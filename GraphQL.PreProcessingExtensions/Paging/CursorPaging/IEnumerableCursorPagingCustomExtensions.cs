﻿# nullable enable

using HotChocolate.Types.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.PreProcessingExtensions.Pagination;

namespace HotChocolate.PreProcessingExtensions
{
    public static class IEnumerableCursorPagingCustomExtensions
    {
        /// <summary>
        /// Implement Linq in-memory slicing as described by Relay spec here:
        /// https://relay.dev/graphql/connections.htm#sec-Pagination-algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="pagingArgs"></param>
        /// <returns></returns>
        public static ICursorPageSlice<T> SliceAsCursorPage<T>(this IEnumerable<T> items, CursorPagingArguments graphQLPagingArgs)
        {
            return items.SliceAsCursorPage(
                after: graphQLPagingArgs.After,
                first: graphQLPagingArgs.First,
                before: graphQLPagingArgs.Before,
                last: graphQLPagingArgs.Last
            );
        }

        /// <summary>
        /// Implement Linq in-memory slicing as described by Relay spec here:
        /// https://relay.dev/graphql/connections.htm#sec-Pagination-algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="pagingArgs"></param>
        /// <param name="after"></param>
        /// <param name="first"></param>
        /// <param name="before"></param>
        /// <param name="last"></param>
        /// <returns></returns>
        public static ICursorPageSlice<T> SliceAsCursorPage<T>(this IEnumerable<T> items, string? after, int? first, string? before, int? last)
        {
            //Do nothing if there are no results...
            if (!items.Any())
                return new CursorPageSlice<T>(new List<ICursorResult<T>>(), 0);

            var afterIndex = after != null
                ? IndexEdge<string>.DeserializeCursor(after)
                : 0;

            var beforeIndex = before != null
                ? IndexEdge<string>.DeserializeCursor(before)
                : 0;

            //FIRST log the index of all items in the list BEFORE slicing, as these indexes are 
            //  the Cursor Indexes for paging up/down the entire list, & ICursorResult is the Decorator 
            //  around the Entity Models.

            //NOTE: We MUST materialize this after applying index values to prevent ongoing increments...
            var itemsList = items.ToList();

            int index = 0;
            IEnumerable<ICursorResult<T>> slice = itemsList
                .Select(c => new CursorResult<T>(c, ++index));

            //If After specified, remove all before After (or skip past After)
            if (afterIndex > 0 && slice.Last().CursorIndex > afterIndex)
            {
                slice = slice.Skip(afterIndex);
            }

            //If Before is specified, remove all after Before (Skip Until Before is reached)
            if (beforeIndex > 0 && slice.Last().CursorIndex > beforeIndex)
            {
                slice = slice.SkipWhile(c => c.CursorIndex < beforeIndex);
            }

            //If First is specified, then take the first/top rows from the current Slice!
            if (first.HasValue && first > 0 && slice.Count() > first)
            {
                slice = slice.Take(first.Value);
            }

            //If First is specified, then take the first/top rows from the current Slice!
            if (last.HasValue && last > 0 && slice.Count() > last)
            {
                slice = slice.TakeLast(last.Value);
            }

            var finalizedSlice = slice.ToList();
            int totalCount = itemsList.Count;

            //Wrap all results into a PagedCursor Slice result wit Total Count...
            //NOTE: to ensure our pagination is complete, we materialize the Results!
            var cursorPageSlice = new CursorPageSlice<T>(finalizedSlice, totalCount);
            return cursorPageSlice;
        }
    }
}
