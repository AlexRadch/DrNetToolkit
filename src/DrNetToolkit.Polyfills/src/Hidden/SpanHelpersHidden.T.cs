﻿// Licensed to the "DrNet Tips&Tricks" under one or more agreements.
// The "DrNet Tips&Tricks" licenses this file to you under the MIT license.
// See the License.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

namespace DrNetToolkit.Polyfills.Hidden;

public static partial class SpanHelpersHidden // .T
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public static int IndexOf<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>?
    {
        Debug.Assert(searchSpaceLength >= 0);
        Debug.Assert(valueLength >= 0);

        if (valueLength == 0)
            return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

        T valueHead = value;
        ref T valueTail = ref Unsafe.Add(ref value, 1);
        int valueTailLength = valueLength - 1;

        int index = 0;
        while (true)
        {
            Debug.Assert(0 <= index && index <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
            int remainingSearchSpaceLength = searchSpaceLength - index - valueTailLength;
            if (remainingSearchSpaceLength <= 0)
                break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

            // Do a quick search for the first element of "value".
            int relativeIndex = IndexOf(ref Unsafe.Add(ref searchSpace, index), valueHead, remainingSearchSpaceLength);
            if (relativeIndex < 0)
                break;
            index += relativeIndex;

            // Found the first element of "value". See if the tail matches.
            if (SequenceEqual(ref Unsafe.Add(ref searchSpace, index + 1), ref valueTail, valueTailLength))
                return index;  // The tail matched. Return a successful find.

            index++;
        }
        return -1;
    }

    public static unsafe bool Contains<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>?
    {
        Debug.Assert(length >= 0);

        nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations

        if (default(T) != null || (object?)value != null)
        {
            Debug.Assert(value is not null);

            while (length >= 8)
            {
                length -= 8;

                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 0)) ||
                    value!.Equals(Unsafe.Add(ref searchSpace, index + 1)) ||
                    value!.Equals(Unsafe.Add(ref searchSpace, index + 2)) ||
                    value!.Equals(Unsafe.Add(ref searchSpace, index + 3)) ||
                    value!.Equals(Unsafe.Add(ref searchSpace, index + 4)) ||
                    value!.Equals(Unsafe.Add(ref searchSpace, index + 5)) ||
                    value!.Equals(Unsafe.Add(ref searchSpace, index + 6)) ||
                    value!.Equals(Unsafe.Add(ref searchSpace, index + 7)))
                {
                    goto Found;
                }

                index += 8;
            }

            if (length >= 4)
            {
                length -= 4;

                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 0)) ||
                    value!.Equals(Unsafe.Add(ref searchSpace, index + 1)) ||
                    value!.Equals(Unsafe.Add(ref searchSpace, index + 2)) ||
                    value!.Equals(Unsafe.Add(ref searchSpace, index + 3)))
                {
                    goto Found;
                }

                index += 4;
            }

            while (length > 0)
            {
                length--;

                if (value!.Equals(Unsafe.Add(ref searchSpace, index)))
                    goto Found;

                index += 1;
            }
        }
        else
        {
            nint len = length;
            for (index = 0; index < len; index++)
            {
                if ((object?)Unsafe.Add(ref searchSpace, index) is null)
                {
                    goto Found;
                }
            }
        }

        return false;

    Found:
        return true;
    }

    public static unsafe int IndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>?
    {
        Debug.Assert(length >= 0);

        nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
        if (default(T) != null || (object?)value != null)
        {
            Debug.Assert(value is not null);

            while (length >= 8)
            {
                length -= 8;

                if (value!.Equals(Unsafe.Add(ref searchSpace, index)))
                    goto Found;
                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 1)))
                    goto Found1;
                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 2)))
                    goto Found2;
                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 3)))
                    goto Found3;
                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 4)))
                    goto Found4;
                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 5)))
                    goto Found5;
                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 6)))
                    goto Found6;
                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 7)))
                    goto Found7;

                index += 8;
            }

            if (length >= 4)
            {
                length -= 4;

                if (value!.Equals(Unsafe.Add(ref searchSpace, index)))
                    goto Found;
                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 1)))
                    goto Found1;
                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 2)))
                    goto Found2;
                if (value!.Equals(Unsafe.Add(ref searchSpace, index + 3)))
                    goto Found3;

                index += 4;
            }

            while (length > 0)
            {
                if (value!.Equals(Unsafe.Add(ref searchSpace, index)))
                    goto Found;

                index += 1;
                length--;
            }
        }
        else
        {
            nint len = (nint)length;
            for (index = 0; index < len; index++)
            {
                if ((object?)Unsafe.Add(ref searchSpace, index) is null)
                {
                    goto Found;
                }
            }
        }
        return -1;

    Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
        return (int)index;
    Found1:
        return (int)(index + 1);
    Found2:
        return (int)(index + 2);
    Found3:
        return (int)(index + 3);
    Found4:
        return (int)(index + 4);
    Found5:
        return (int)(index + 5);
    Found6:
        return (int)(index + 6);
    Found7:
        return (int)(index + 7);
    }

    public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>?
    {
        Debug.Assert(length >= 0);

        T lookUp;
        int index = 0;
        if (default(T) != null || ((object?)value0 != null && (object?)value1 != null))
        {
            Debug.Assert(value0 is not null && value1 is not null);

            while ((length - index) >= 8)
            {
                lookUp = Unsafe.Add(ref searchSpace, index);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp))
                    goto Found;
                lookUp = Unsafe.Add(ref searchSpace, index + 1);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, index + 2);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, index + 3);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found3;
                lookUp = Unsafe.Add(ref searchSpace, index + 4);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found4;
                lookUp = Unsafe.Add(ref searchSpace, index + 5);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found5;
                lookUp = Unsafe.Add(ref searchSpace, index + 6);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found6;
                lookUp = Unsafe.Add(ref searchSpace, index + 7);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found7;

                index += 8;
            }

            if ((length - index) >= 4)
            {
                lookUp = Unsafe.Add(ref searchSpace, index);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp))
                    goto Found;
                lookUp = Unsafe.Add(ref searchSpace, index + 1);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, index + 2);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, index + 3);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found3;

                index += 4;
            }

            while (index < length)
            {
                lookUp = Unsafe.Add(ref searchSpace, index);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp))
                    goto Found;

                index++;
            }
        }
        else
        {
            for (index = 0; index < length; index++)
            {
                lookUp = Unsafe.Add(ref searchSpace, index);
                if ((object?)lookUp is null)
                {
                    if ((object?)value0 is null || (object?)value1 is null)
                    {
                        goto Found;
                    }
                }
                else if (lookUp.Equals(value0) || lookUp.Equals(value1))
                {
                    goto Found;
                }
            }
        }

        return -1;

    Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
        return index;
    Found1:
        return index + 1;
    Found2:
        return index + 2;
    Found3:
        return index + 3;
    Found4:
        return index + 4;
    Found5:
        return index + 5;
    Found6:
        return index + 6;
    Found7:
        return index + 7;
    }

    public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : IEquatable<T>?
    {
        Debug.Assert(length >= 0);

        T lookUp;
        int index = 0;
        if (default(T) != null || ((object?)value0 != null && (object?)value1 != null && (object?)value2 != null))
        {
            Debug.Assert(value0 is not null && value1 is not null && value2 is not null);

            while ((length - index) >= 8)
            {
                lookUp = Unsafe.Add(ref searchSpace, index);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp) || value2!.Equals(lookUp))
                    goto Found;
                lookUp = Unsafe.Add(ref searchSpace, index + 1);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, index + 2);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, index + 3);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found3;
                lookUp = Unsafe.Add(ref searchSpace, index + 4);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found4;
                lookUp = Unsafe.Add(ref searchSpace, index + 5);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found5;
                lookUp = Unsafe.Add(ref searchSpace, index + 6);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found6;
                lookUp = Unsafe.Add(ref searchSpace, index + 7);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found7;

                index += 8;
            }

            if ((length - index) >= 4)
            {
                lookUp = Unsafe.Add(ref searchSpace, index);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp) || value2!.Equals(lookUp))
                    goto Found;
                lookUp = Unsafe.Add(ref searchSpace, index + 1);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, index + 2);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, index + 3);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found3;

                index += 4;
            }

            while (index < length)
            {
                lookUp = Unsafe.Add(ref searchSpace, index);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp) || value2!.Equals(lookUp))
                    goto Found;

                index++;
            }
        }
        else
        {
            for (index = 0; index < length; index++)
            {
                lookUp = Unsafe.Add(ref searchSpace, index);
                if ((object?)lookUp is null)
                {
                    if ((object?)value0 is null || (object?)value1 is null || (object?)value2 is null)
                    {
                        goto Found;
                    }
                }
                else if (lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))
                {
                    goto Found;
                }
            }
        }
        return -1;

    Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
        return index;
    Found1:
        return index + 1;
    Found2:
        return index + 2;
    Found3:
        return index + 3;
    Found4:
        return index + 4;
    Found5:
        return index + 5;
    Found6:
        return index + 6;
    Found7:
        return index + 7;
    }

    public static int IndexOfAny<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>?
    {
        Debug.Assert(searchSpaceLength >= 0);
        Debug.Assert(valueLength >= 0);

        if (valueLength == 0)
            return -1;  // A zero-length set of values is always treated as "not found".

        // For the following paragraph, let:
        //   n := length of haystack
        //   i := index of first occurrence of any needle within haystack
        //   l := length of needle array
        //
        // We use a naive non-vectorized search because we want to bound the complexity of IndexOfAny
        // to O(i * l) rather than O(n * l), or just O(n * l) if no needle is found. The reason for
        // this is that it's common for callers to invoke IndexOfAny immediately before slicing,
        // and when this is called in a loop, we want the entire loop to be bounded by O(n * l)
        // rather than O(n^2 * l).

        if (typeof(T).IsValueType())
        {
            // Calling ValueType.Equals (devirtualized), which takes 'this' byref. We'll make
            // a byval copy of the candidate from the search space in the outer loop, then in
            // the inner loop we'll pass a ref (as 'this') to each element in the needle.

            for (int i = 0; i < searchSpaceLength; i++)
            {
                T candidate = Unsafe.Add(ref searchSpace, i);
                for (int j = 0; j < valueLength; j++)
                {
                    if (Unsafe.Add(ref value, j)!.Equals(candidate))
                    {
                        return i;
                    }
                }
            }
        }
        else
        {
            // Calling IEquatable<T>.Equals (virtual dispatch). We'll perform the null check
            // in the outer loop instead of in the inner loop to save some branching.

            for (int i = 0; i < searchSpaceLength; i++)
            {
                T candidate = Unsafe.Add(ref searchSpace, i);
                if (candidate is not null)
                {
                    for (int j = 0; j < valueLength; j++)
                    {
                        if (candidate.Equals(Unsafe.Add(ref value, j)))
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < valueLength; j++)
                    {
                        if (Unsafe.Add(ref value, j) is null)
                        {
                            return i;
                        }
                    }
                }
            }
        }

        return -1; // not found
    }

    public static int LastIndexOf<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>?
    {
        Debug.Assert(searchSpaceLength >= 0);
        Debug.Assert(valueLength >= 0);

        if (valueLength == 0)
            return searchSpaceLength;  // A zero-length sequence is always treated as "found" at the end of the search space.

        int valueTailLength = valueLength - 1;
        if (valueTailLength == 0)
        {
            return LastIndexOf(ref searchSpace, value, searchSpaceLength);
        }

        int index = 0;

        T valueHead = value;
        ref T valueTail = ref Unsafe.Add(ref value, 1);

        while (true)
        {
            Debug.Assert(0 <= index && index <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
            int remainingSearchSpaceLength = searchSpaceLength - index - valueTailLength;
            if (remainingSearchSpaceLength <= 0)
                break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

            // Do a quick search for the first element of "value".
            int relativeIndex = LastIndexOf(ref searchSpace, valueHead, remainingSearchSpaceLength);
            if (relativeIndex < 0)
                break;

            // Found the first element of "value". See if the tail matches.
            if (SequenceEqual(ref Unsafe.Add(ref searchSpace, relativeIndex + 1), ref valueTail, valueTailLength))
                return relativeIndex;  // The tail matched. Return a successful find.

            index += remainingSearchSpaceLength - relativeIndex;
        }
        return -1;
    }

    public static int LastIndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>?
    {
        Debug.Assert(length >= 0);

        if (default(T) != null || (object?)value != null)
        {
            Debug.Assert(value is not null);

            while (length >= 8)
            {
                length -= 8;

                if (value!.Equals(Unsafe.Add(ref searchSpace, length + 7)))
                    goto Found7;
                if (value.Equals(Unsafe.Add(ref searchSpace, length + 6)))
                    goto Found6;
                if (value.Equals(Unsafe.Add(ref searchSpace, length + 5)))
                    goto Found5;
                if (value.Equals(Unsafe.Add(ref searchSpace, length + 4)))
                    goto Found4;
                if (value.Equals(Unsafe.Add(ref searchSpace, length + 3)))
                    goto Found3;
                if (value.Equals(Unsafe.Add(ref searchSpace, length + 2)))
                    goto Found2;
                if (value.Equals(Unsafe.Add(ref searchSpace, length + 1)))
                    goto Found1;
                if (value.Equals(Unsafe.Add(ref searchSpace, length)))
                    goto Found;
            }

            if (length >= 4)
            {
                length -= 4;

                if (value!.Equals(Unsafe.Add(ref searchSpace, length + 3)))
                    goto Found3;
                if (value.Equals(Unsafe.Add(ref searchSpace, length + 2)))
                    goto Found2;
                if (value.Equals(Unsafe.Add(ref searchSpace, length + 1)))
                    goto Found1;
                if (value.Equals(Unsafe.Add(ref searchSpace, length)))
                    goto Found;
            }

            while (length > 0)
            {
                length--;

                if (value!.Equals(Unsafe.Add(ref searchSpace, length)))
                    goto Found;
            }
        }
        else
        {
            for (length--; length >= 0; length--)
            {
                if ((object?)Unsafe.Add(ref searchSpace, length) is null)
                {
                    goto Found;
                }
            }
        }

        return -1;

    Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
        return length;
    Found1:
        return length + 1;
    Found2:
        return length + 2;
    Found3:
        return length + 3;
    Found4:
        return length + 4;
    Found5:
        return length + 5;
    Found6:
        return length + 6;
    Found7:
        return length + 7;
    }

    public static int LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>?
    {
        Debug.Assert(length >= 0);

        T lookUp;
        if (default(T) != null || ((object?)value0 != null && (object?)value1 != null))
        {
            Debug.Assert(value0 is not null && value1 is not null);

            while (length >= 8)
            {
                length -= 8;

                lookUp = Unsafe.Add(ref searchSpace, length + 7);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp))
                    goto Found7;
                lookUp = Unsafe.Add(ref searchSpace, length + 6);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found6;
                lookUp = Unsafe.Add(ref searchSpace, length + 5);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found5;
                lookUp = Unsafe.Add(ref searchSpace, length + 4);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found4;
                lookUp = Unsafe.Add(ref searchSpace, length + 3);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found3;
                lookUp = Unsafe.Add(ref searchSpace, length + 2);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, length + 1);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found;
            }

            if (length >= 4)
            {
                length -= 4;

                lookUp = Unsafe.Add(ref searchSpace, length + 3);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp))
                    goto Found3;
                lookUp = Unsafe.Add(ref searchSpace, length + 2);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, length + 1);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0.Equals(lookUp) || value1.Equals(lookUp))
                    goto Found;
            }

            while (length > 0)
            {
                length--;

                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp))
                    goto Found;
            }
        }
        else
        {
            for (length--; length >= 0; length--)
            {
                lookUp = Unsafe.Add(ref searchSpace, length);
                if ((object?)lookUp is null)
                {
                    if ((object?)value0 is null || (object?)value1 is null)
                    {
                        goto Found;
                    }
                }
                else if (lookUp.Equals(value0) || lookUp.Equals(value1))
                {
                    goto Found;
                }
            }
        }

        return -1;

    Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
        return length;
    Found1:
        return length + 1;
    Found2:
        return length + 2;
    Found3:
        return length + 3;
    Found4:
        return length + 4;
    Found5:
        return length + 5;
    Found6:
        return length + 6;
    Found7:
        return length + 7;
    }

    public static int LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : IEquatable<T>?
    {
        Debug.Assert(length >= 0);

        T lookUp;
        if (default(T) != null || ((object?)value0 != null && (object?)value1 != null && (object?)value2 != null))
        {
            Debug.Assert(value0 is not null && value1 is not null && value2 is not null);

            while (length >= 8)
            {
                length -= 8;

                lookUp = Unsafe.Add(ref searchSpace, length + 7);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp) || value2!.Equals(lookUp))
                    goto Found7;
                lookUp = Unsafe.Add(ref searchSpace, length + 6);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found6;
                lookUp = Unsafe.Add(ref searchSpace, length + 5);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found5;
                lookUp = Unsafe.Add(ref searchSpace, length + 4);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found4;
                lookUp = Unsafe.Add(ref searchSpace, length + 3);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found3;
                lookUp = Unsafe.Add(ref searchSpace, length + 2);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, length + 1);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found;
            }

            if (length >= 4)
            {
                length -= 4;

                lookUp = Unsafe.Add(ref searchSpace, length + 3);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp) || value2!.Equals(lookUp))
                    goto Found3;
                lookUp = Unsafe.Add(ref searchSpace, length + 2);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found2;
                lookUp = Unsafe.Add(ref searchSpace, length + 1);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found1;
                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                    goto Found;
            }

            while (length > 0)
            {
                length--;

                lookUp = Unsafe.Add(ref searchSpace, length);
                if (value0!.Equals(lookUp) || value1!.Equals(lookUp) || value2!.Equals(lookUp))
                    goto Found;
            }
        }
        else
        {
            for (length--; length >= 0; length--)
            {
                lookUp = Unsafe.Add(ref searchSpace, length);
                if ((object?)lookUp is null)
                {
                    if ((object?)value0 is null || (object?)value1 is null || (object?)value2 is null)
                    {
                        goto Found;
                    }
                }
                else if (lookUp.Equals(value0) || lookUp.Equals(value1) || lookUp.Equals(value2))
                {
                    goto Found;
                }
            }
        }

        return -1;

    Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
        return length;
    Found1:
        return length + 1;
    Found2:
        return length + 2;
    Found3:
        return length + 3;
    Found4:
        return length + 4;
    Found5:
        return length + 5;
    Found6:
        return length + 6;
    Found7:
        return length + 7;
    }

    public static int LastIndexOfAny<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>?
    {
        Debug.Assert(searchSpaceLength >= 0);
        Debug.Assert(valueLength >= 0);

        if (valueLength == 0)
            return -1;  // A zero-length set of values is always treated as "not found".

        // See comments in IndexOfAny(ref T, int, ref T, int) above regarding algorithmic complexity concerns.
        // This logic is similar, but it runs backward.

        if (typeof(T).IsValueType())
        {
            for (int i = searchSpaceLength - 1; i >= 0; i--)
            {
                T candidate = Unsafe.Add(ref searchSpace, i);
                for (int j = 0; j < valueLength; j++)
                {
                    if (Unsafe.Add(ref value, j)!.Equals(candidate))
                    {
                        return i;
                    }
                }
            }
        }
        else
        {
            for (int i = searchSpaceLength - 1; i >= 0; i--)
            {
                T candidate = Unsafe.Add(ref searchSpace, i);
                if (candidate is not null)
                {
                    for (int j = 0; j < valueLength; j++)
                    {
                        if (candidate.Equals(Unsafe.Add(ref value, j)))
                        {
                            return i;
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < valueLength; j++)
                    {
                        if (Unsafe.Add(ref value, j) is null)
                        {
                            return i;
                        }
                    }
                }
            }
        }

        return -1; // not found
    }

    public static bool SequenceEqual<T>(ref T first, ref T second, int length) where T : IEquatable<T>?
    {
        Debug.Assert(length >= 0);

        if (Unsafe.AreSame(ref first, ref second))
            goto Equal;

        nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
        T lookUp0;
        T lookUp1;
        while (length >= 8)
        {
            length -= 8;

            lookUp0 = Unsafe.Add(ref first, index);
            lookUp1 = Unsafe.Add(ref second, index);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 1);
            lookUp1 = Unsafe.Add(ref second, index + 1);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 2);
            lookUp1 = Unsafe.Add(ref second, index + 2);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 3);
            lookUp1 = Unsafe.Add(ref second, index + 3);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 4);
            lookUp1 = Unsafe.Add(ref second, index + 4);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 5);
            lookUp1 = Unsafe.Add(ref second, index + 5);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 6);
            lookUp1 = Unsafe.Add(ref second, index + 6);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 7);
            lookUp1 = Unsafe.Add(ref second, index + 7);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;

            index += 8;
        }

        if (length >= 4)
        {
            length -= 4;

            lookUp0 = Unsafe.Add(ref first, index);
            lookUp1 = Unsafe.Add(ref second, index);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 1);
            lookUp1 = Unsafe.Add(ref second, index + 1);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 2);
            lookUp1 = Unsafe.Add(ref second, index + 2);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 3);
            lookUp1 = Unsafe.Add(ref second, index + 3);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;

            index += 4;
        }

        while (length > 0)
        {
            lookUp0 = Unsafe.Add(ref first, index);
            lookUp1 = Unsafe.Add(ref second, index);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                goto NotEqual;
            index += 1;
            length--;
        }

    Equal:
        return true;

    NotEqual: // Workaround for https://github.com/dotnet/runtime/issues/8795
        return false;
    }

    public static int IndexOfAnyExcept<T>(ref T searchSpace, T value0, int length)
    {
        Debug.Assert(length >= 0, "Expected non-negative length");

        for (int i = 0; i < length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(Unsafe.Add(ref searchSpace, i), value0))
            {
                return i;
            }
        }

        return -1;
    }

    public static int LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, int length)
    {
        Debug.Assert(length >= 0, "Expected non-negative length");

        for (int i = length - 1; i >= 0; i--)
        {
            if (!EqualityComparer<T>.Default.Equals(Unsafe.Add(ref searchSpace, i), value0))
            {
                return i;
            }
        }

        return -1;
    }

    public static int IndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, int length)
    {
        Debug.Assert(length >= 0, "Expected non-negative length");

        for (int i = 0; i < length; i++)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, i);
            if (!EqualityComparer<T>.Default.Equals(current, value0) && !EqualityComparer<T>.Default.Equals(current, value1))
            {
                return i;
            }
        }

        return -1;
    }

    public static int LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, int length)
    {
        Debug.Assert(length >= 0, "Expected non-negative length");

        for (int i = length - 1; i >= 0; i--)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, i);
            if (!EqualityComparer<T>.Default.Equals(current, value0) && !EqualityComparer<T>.Default.Equals(current, value1))
            {
                return i;
            }
        }

        return -1;
    }

    public static int IndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, int length)
    {
        Debug.Assert(length >= 0, "Expected non-negative length");

        for (int i = 0; i < length; i++)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, i);
            if (!EqualityComparer<T>.Default.Equals(current, value0)
                && !EqualityComparer<T>.Default.Equals(current, value1)
                && !EqualityComparer<T>.Default.Equals(current, value2))
            {
                return i;
            }
        }

        return -1;
    }

    public static int LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, int length)
    {
        Debug.Assert(length >= 0, "Expected non-negative length");

        for (int i = length - 1; i >= 0; i--)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, i);
            if (!EqualityComparer<T>.Default.Equals(current, value0)
                && !EqualityComparer<T>.Default.Equals(current, value1)
                && !EqualityComparer<T>.Default.Equals(current, value2))
            {
                return i;
            }
        }

        return -1;
    }

    public static int IndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, T value3, int length)
    {
        Debug.Assert(length >= 0, "Expected non-negative length");

        for (int i = 0; i < length; i++)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, i);
            if (!EqualityComparer<T>.Default.Equals(current, value0)
                && !EqualityComparer<T>.Default.Equals(current, value1)
                && !EqualityComparer<T>.Default.Equals(current, value2)
                && !EqualityComparer<T>.Default.Equals(current, value3))
            {
                return i;
            }
        }

        return -1;
    }

    public static int LastIndexOfAnyExcept<T>(ref T searchSpace, T value0, T value1, T value2, T value3, int length)
    {
        Debug.Assert(length >= 0, "Expected non-negative length");

        for (int i = length - 1; i >= 0; i--)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, i);
            if (!EqualityComparer<T>.Default.Equals(current, value0)
                && !EqualityComparer<T>.Default.Equals(current, value1)
                && !EqualityComparer<T>.Default.Equals(current, value2)
                && !EqualityComparer<T>.Default.Equals(current, value3))
            {
                return i;
            }
        }

        return -1;
    }

    public static int SequenceCompareTo<T>(ref T first, int firstLength, ref T second, int secondLength)
        where T : IComparable<T>?
    {
        Debug.Assert(firstLength >= 0);
        Debug.Assert(secondLength >= 0);

        int minLength = firstLength;
        if (minLength > secondLength)
            minLength = secondLength;
        for (int i = 0; i < minLength; i++)
        {
            T lookUp = Unsafe.Add(ref second, i);
            int result = (Unsafe.Add(ref first, i)?.CompareTo(lookUp) ?? (((object?)lookUp is null) ? 0 : -1));
            if (result != 0)
                return result;
        }
        return firstLength.CompareTo(secondLength);
    }

#if NET7_0_OR_GREATER
    public static bool NonPackedContainsValueType<T>(ref T searchSpace, T value, int length) where T : struct, INumber<T>
    {
        Debug.Assert(length >= 0, "Expected non-negative length");
        Debug.Assert(value is byte or short or int or long, "Expected caller to normalize to one of these types");

        if (!Vector128.IsHardwareAccelerated || length < Vector128<T>.Count)
        {
            nuint offset = 0;

            while (length >= 8)
            {
                length -= 8;

                if (Unsafe.Add(ref searchSpace, offset) == value
                 || Unsafe.Add(ref searchSpace, offset + 1) == value
                 || Unsafe.Add(ref searchSpace, offset + 2) == value
                 || Unsafe.Add(ref searchSpace, offset + 3) == value
                 || Unsafe.Add(ref searchSpace, offset + 4) == value
                 || Unsafe.Add(ref searchSpace, offset + 5) == value
                 || Unsafe.Add(ref searchSpace, offset + 6) == value
                 || Unsafe.Add(ref searchSpace, offset + 7) == value)
                {
                    return true;
                }

                offset += 8;
            }

            if (length >= 4)
            {
                length -= 4;

                if (Unsafe.Add(ref searchSpace, offset) == value
                 || Unsafe.Add(ref searchSpace, offset + 1) == value
                 || Unsafe.Add(ref searchSpace, offset + 2) == value
                 || Unsafe.Add(ref searchSpace, offset + 3) == value)
                {
                    return true;
                }

                offset += 4;
            }

            while (length > 0)
            {
                length -= 1;

                if (Unsafe.Add(ref searchSpace, offset) == value) return true;

                offset += 1;
            }
        }
#if NET8_0_OR_GREATER
        else if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> current, values = Vector512.Create(value);
            ref T currentSearchSpace = ref searchSpace;
            ref T oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(length - Vector512<T>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector512.LoadUnsafe(ref currentSearchSpace);

                if (Vector512.EqualsAny(values, current))
                {
                    return true;
                }

                currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector512<T>.Count);
            }
            while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

            // If any elements remain, process the last vector in the search space.
            if ((uint)length % Vector512<T>.Count != 0)
            {
                current = Vector512.LoadUnsafe(ref oneVectorAwayFromEnd);

                if (Vector512.EqualsAny(values, current))
                {
                    return true;
                }
            }
        }
#endif
        else if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            Vector256<T> equals, values = Vector256.Create(value);
            ref T currentSearchSpace = ref searchSpace;
            ref T oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(length - Vector256<T>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                equals = Vector256.Equals(values, Vector256.LoadUnsafe(ref currentSearchSpace));
                if (equals == Vector256<T>.Zero)
                {
                    currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector256<T>.Count);
                    continue;
                }

                return true;
            }
            while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

            // If any elements remain, process the last vector in the search space.
            if ((uint)length % Vector256<T>.Count != 0)
            {
                equals = Vector256.Equals(values, Vector256.LoadUnsafe(ref oneVectorAwayFromEnd));
                if (equals != Vector256<T>.Zero)
                {
                    return true;
                }
            }
        }
        else
        {
            Vector128<T> equals, values = Vector128.Create(value);
            ref T currentSearchSpace = ref searchSpace;
            ref T oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(length - Vector128<T>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                equals = Vector128.Equals(values, Vector128.LoadUnsafe(ref currentSearchSpace));
                if (equals == Vector128<T>.Zero)
                {
                    currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector128<T>.Count);
                    continue;
                }

                return true;
            }
            while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

            // If any elements remain, process the first vector in the search space.
            if ((uint)length % Vector128<T>.Count != 0)
            {
                equals = Vector128.Equals(values, Vector128.LoadUnsafe(ref oneVectorAwayFromEnd));
                if (equals != Vector128<T>.Zero)
                {
                    return true;
                }
            }
        }

        return false;
    }
#endif

        [CLSCompliant(false)]
    public static void Replace<T>(ref T src, ref T dst, T oldValue, T newValue, int length) where T : IEquatable<T>?
    {
        if (default(T) is not null || oldValue is not null)
        {
            Debug.Assert(oldValue is not null);

#if NETSTANDARD2_0_OR_GREATER
            for (nuint idx = 0; idx < (nuint)length; ++idx)
#else
            for (int idx = 0; idx < length; ++idx)
#endif
            {
                T original = Unsafe.Add(ref src, idx);
                Unsafe.Add(ref dst, idx) = oldValue!.Equals(original) ? newValue : original;
            }
        }
        else
        {
#if NETSTANDARD2_0_OR_GREATER
            for (nuint idx = 0; idx < (nuint)length; ++idx)
#else
            for (int idx = 0; idx < length; ++idx)
#endif
            {
                T original = Unsafe.Add(ref src, idx);
                Unsafe.Add(ref dst, idx) = original is null ? newValue : original;
            }
        }
    }

    public static int IndexOfAnyInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, int length)
        where T : IComparable<T>
    {
        for (int i = 0; i < length; i++)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, i);
            if ((lowInclusive.CompareTo(current) <= 0) && (highInclusive.CompareTo(current) >= 0))
            {
                return i;
            }
        }

        return -1;
    }

    public static int IndexOfAnyExceptInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, int length)
        where T : IComparable<T>
    {
        for (int i = 0; i < length; i++)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, i);
            if ((lowInclusive.CompareTo(current) > 0) || (highInclusive.CompareTo(current) < 0))
            {
                return i;
            }
        }

        return -1;
    }

    public static int LastIndexOfAnyInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, int length)
        where T : IComparable<T>
    {
        for (int i = length - 1; i >= 0; i--)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, i);
            if ((lowInclusive.CompareTo(current) <= 0) && (highInclusive.CompareTo(current) >= 0))
            {
                return i;
            }
        }

        return -1;
    }

    public static int LastIndexOfAnyExceptInRange<T>(ref T searchSpace, T lowInclusive, T highInclusive, int length)
        where T : IComparable<T>
    {
        for (int i = length - 1; i >= 0; i--)
        {
            ref T current = ref Unsafe.Add(ref searchSpace, i);
            if ((lowInclusive.CompareTo(current) > 0) || (highInclusive.CompareTo(current) < 0))
            {
                return i;
            }
        }

        return -1;
    }

#if NET7_0_OR_GREATER
    [CLSCompliant(false)]
    public interface INegator<T> where T : struct
    {
        static abstract bool NegateIfNeeded(bool equals);
        static abstract Vector128<T> NegateIfNeeded(Vector128<T> equals);
        static abstract Vector256<T> NegateIfNeeded(Vector256<T> equals);
#if NET8_0_OR_GREATER
        static abstract Vector512<T> NegateIfNeeded(Vector512<T> equals);
#endif

        // The generic vector APIs assume use for IndexOf where `DontNegate` is
        // for `IndexOfAny` and `Negate` is for `IndexOfAnyExcept`

        static abstract bool HasMatch<TVector>(TVector left, TVector right)
            where TVector : struct, ISimdVectorHidden<TVector, T>;

        static abstract TVector GetMatchMask<TVector>(TVector left, TVector right)
            where TVector : struct, ISimdVectorHidden<TVector, T>;
    }

    public readonly struct DontNegate<T> : INegator<T>
        where T : struct
    {
        public static bool NegateIfNeeded(bool equals) => equals;
        public static Vector128<T> NegateIfNeeded(Vector128<T> equals) => equals;
        public static Vector256<T> NegateIfNeeded(Vector256<T> equals) => equals;
#if NET8_0_OR_GREATER
        public static Vector512<T> NegateIfNeeded(Vector512<T> equals) => equals;
#endif

        // The generic vector APIs assume use for `IndexOfAny` where we
        // want "HasMatch" to mean any of the two elements match.

        [CLSCompliant(false)]
        public static bool HasMatch<TVector>(TVector left, TVector right)
            where TVector : struct, ISimdVectorHidden<TVector, T>
        {
            return TVector.EqualsAny(left, right);
        }

        [CLSCompliant(false)]
        public static TVector GetMatchMask<TVector>(TVector left, TVector right)
            where TVector : struct, ISimdVectorHidden<TVector, T>
        {
            return TVector.Equals(left, right);
        }
    }

    public readonly struct Negate<T> : INegator<T>
        where T : struct
    {
        public static bool NegateIfNeeded(bool equals) => !equals;
        public static Vector128<T> NegateIfNeeded(Vector128<T> equals) => ~equals;
        public static Vector256<T> NegateIfNeeded(Vector256<T> equals) => ~equals;
#if NET8_0_OR_GREATER
        public static Vector512<T> NegateIfNeeded(Vector512<T> equals) => ~equals;
#endif

        // The generic vector APIs assume use for `IndexOfAnyExcept` where we
        // want "HasMatch" to mean any of the two elements don't match

        [CLSCompliant(false)]
        public static bool HasMatch<TVector>(TVector left, TVector right)
            where TVector : struct, ISimdVectorHidden<TVector, T>
        {
            return !TVector.EqualsAll(left, right);
        }

        [CLSCompliant(false)]
        public static TVector GetMatchMask<TVector>(TVector left, TVector right)
            where TVector : struct, ISimdVectorHidden<TVector, T>
        {
            return ~TVector.Equals(left, right);
        }
    }
#endif

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
