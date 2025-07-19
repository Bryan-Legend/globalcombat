using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
//using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LT
{
    ///// <summary>
    ///// Creates an indexed list.  Requires that [Key] attribute be applied to a property in TValue object.
    ///// </summary>
    ///// <example>
    ///// public class Test
    ///// {
    /////     [Key]
    /////     public int Id { get; set; }
    ///// }
    ///// 
    ///// IndexedList<int, Test> tests;
    ///// </example>
    ///// <typeparam name="TKey"></typeparam>
    ///// <typeparam name="TValue"></typeparam>
    public class IndexedList<TKey, TValue> : KeyedCollection<TKey, TValue>
    {
        PropertyInfo keyProperty;

        public IndexedList()
        {
            foreach (var property in typeof(TValue).GetProperties())
            {
                // this requires .net 4, which I couldn't use due to the WPF shadow effect deprication
                //if (property.PropertyType == typeof(TKey) && property.IsDefined(typeof(KeyAttribute), true))

                if (property.PropertyType == typeof(TKey) && (property.Name.ToUpper() == "ID" || property.Name.ToUpper() == "KEY"))
                {
                    keyProperty = property;
                    return;
                }
            }

            throw new ArgumentException(String.Format("Unable to find a property in {0} that is named Id or Key and is of type {1}.", typeof(TValue).Name, typeof(TKey).Name));
        }

        protected override TKey GetKeyForItem(TValue item)
        {
            return (TKey)keyProperty.GetValue(item, null);
        }
    }
}
