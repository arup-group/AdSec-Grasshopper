using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xunit;

namespace AdSecGHTests.Helpers {

  public class Duplicates {

    public static bool AreEqual(object objA, object objB, bool excludeGuid = false) {
      if (!(excludeGuid && objA.Equals(typeof(Guid)))) {
        Assert.Equal(objA.ToString(), objB.ToString());
      }

      var typeA = objA.GetType();
      var typeB = objB.GetType();

      var propertyInfoA = typeA.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      var propertyInfoB = typeB.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

      for (int i = 0; i < propertyInfoA.Length; i++) {
        var propertyA = propertyInfoA[i];
        var propertyB = propertyInfoB[i];

        if (!propertyA.CanWrite && !propertyB.CanWrite) {
          continue;
        }

        if (!propertyA.CanWrite || !propertyB.CanWrite) {
          Assert.Equal(objA, objB);
        }

        object objPropertyValueA;
        object objPropertyValueB;
        var propertyTypeA = propertyA.PropertyType;
        var propertyTypeB = propertyB.PropertyType;

        try {
          objPropertyValueA = propertyA.GetValue(objA, null);
          objPropertyValueB = propertyB.GetValue(objB, null);

          // check wether property is an interface
          if (propertyTypeA.IsInterface) {
            if (objPropertyValueA != null) {
              propertyTypeA = objPropertyValueA.GetType();
            }
          }

          if (propertyTypeB.IsInterface) {
            if (objPropertyValueB != null) {
              propertyTypeB = objPropertyValueB.GetType();
            }
          }

          // check wether property is an enumerable
          if (typeof(IEnumerable).IsAssignableFrom(propertyTypeA) && !typeof(string).IsAssignableFrom(propertyTypeA)) {
            if (typeof(IEnumerable).IsAssignableFrom(propertyTypeB)
              && !typeof(string).IsAssignableFrom(propertyTypeB)) {
              if (objPropertyValueA == null || objPropertyValueB == null) {
                Assert.Equal(objPropertyValueA, objPropertyValueB);
              } else {
                var enumerableA = ((IEnumerable)objPropertyValueA).Cast<object>();
                var enumerableB = ((IEnumerable)objPropertyValueB).Cast<object>();

                Type enumrableTypeA = null;
                Type enumrableTypeB = null;
                if (enumerableA.GetType().GetGenericArguments().Length > 0) {
                  enumrableTypeA = enumerableA.GetType().GetGenericArguments()[0];
                }

                if (enumerableB.GetType().GetGenericArguments().Length > 0) {
                  enumrableTypeB = enumerableB.GetType().GetGenericArguments()[0];
                }

                Assert.Equal(enumrableTypeA, enumrableTypeB);

                // if type is a struct, we have to check the actual list items
                // this will fail if list is actually of type "System.Object"..
                if (enumrableTypeA.ToString() is "System.Object") {
                  if (enumerableA.Any()) {
                    enumrableTypeA = enumerableA.First().GetType();
                  } else {
                    continue; // can´t get type of struct in empty list?
                  }
                }

                if (enumrableTypeB.ToString() is "System.Object") {
                  if (enumerableB.Any()) {
                    enumrableTypeB = enumerableB.First().GetType();
                  } else {
                    continue; // can´t get type of struct in empty list?
                  }
                }

                var genericListTypeA = typeof(List<>).MakeGenericType(enumrableTypeA);
                var genericListTypeB = typeof(List<>).MakeGenericType(enumrableTypeB);
                Assert.Equal(genericListTypeA, genericListTypeB);

                var enumeratorB = enumerableB.GetEnumerator();

                using (var enumeratorA = enumerableA.GetEnumerator()) {
                  while (enumeratorA.MoveNext()) {
                    Assert.True(enumeratorB.MoveNext());
                    AreEqual(enumeratorA.Current, enumeratorB.Current);
                  }
                }
              }
            } else {
              Assert.Equal(objPropertyValueA, objPropertyValueB);
            }
          }
          // check whether property type is value type, enum or string type
          else if (propertyTypeA.IsValueType || propertyTypeA.IsEnum || propertyTypeA.Equals(typeof(string))) {
            if (excludeGuid && propertyTypeA.Equals(typeof(Guid))) {
              continue;
            }

            Assert.Equal(objPropertyValueA, objPropertyValueB);
          } else if (objPropertyValueA == null || objPropertyValueB == null) {
            Assert.Equal(objPropertyValueA, objPropertyValueB);
          } else
          // property type is object/complex type, so need to recursively call this method until the end of the tree is reached
          {
            AreEqual(objPropertyValueA, objPropertyValueB, excludeGuid);
          }
        } catch (TargetParameterCountException) {
          propertyTypeA = propertyA.PropertyType;
        }
      }

      return true;
    }
  }
}
