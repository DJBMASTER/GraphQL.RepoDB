﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotChocolate.PreProcessingExtensions
{
    public static class SystemTypeCustomExtensions
    {
        public static readonly Type ObjectType = typeof(object);

        /// <summary>
        /// A wonderful little utility for robust Generic Type comparisons 
        /// more info see: https://stackoverflow.com/a/37184228/7293142
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public static bool IsDerivedFromGenericParent(this Type type, Type parentType)
        {
            if (!parentType.IsGenericType)
            {
                throw new ArgumentException("Type must be generic.", nameof(parentType));
            }
            else if (type == null || type == ObjectType)
            {
                return false;
            }
            else if (
                (type.IsGenericType && type.GetGenericTypeDefinition() == parentType)
                || (type.BaseType.IsDerivedFromGenericParent(parentType))
                //Recursively search for Interfaces...
                || (type.GetInterfaces().Any(t => t.IsDerivedFromGenericParent(parentType)))
            )
            {
                return true;
            }
         
            return false;
        }
    }
}
