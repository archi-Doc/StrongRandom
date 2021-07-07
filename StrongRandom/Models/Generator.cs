﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SA1602 // Enumeration items should be documented

namespace StrongRandom.Models
{
    public enum GenerateId
    {
        String10,
        Guid,
        Hex8,
    }

    public class Generator
    {
        private RNGCryptoServiceProvider provider = new();

        public Generator()
        {
        }

        public string Generate(GenerateId id)
        {
            switch (id)
            {
                case GenerateId.Guid:
                    return Guid.NewGuid().ToString();

                case GenerateId.String10:
                    return this.GenerateString(10);

                case GenerateId.Hex8:
                    return this.GenerateHex(8);

                default:
                    return string.Empty;
            }
        }

        private string GenerateHex(int length)
        {
            var chars = new char[length];

Generate:
            for (var n = 0; n < length; n++)
            {
                chars[n] = this.GetChar(CharKind.Hexadecimal);
            }

            if (!this.ContainsAll(chars, CharKind.Hexadecimal))
            {
                goto Generate;
            }

            return "0x" + new string(chars);
        }

        private string GenerateString(int length)
        {
            var chars = new char[length];

Generate:
            for (var n = 0; n < length; n++)
            {
                chars[n] = this.GetChar(CharKind.NumberAlphabet);
            }

            if (!this.ContainsAll(chars, CharKind.NumberAlphabet))
            {
                goto Generate;
            }

            return new string(chars);
        }

        private enum CharKind
        {
            NumberAlphabet,
            Hexadecimal,
        }

        private char GetChar(CharKind kind)
        {
            uint u;
            switch (kind)
            {
                case CharKind.NumberAlphabet:
                    u = this.GetUInt() % (10 + 26 + 26);
                    if (u < 10)
                    {
                        return (char)('0' + u);
                    }
                    else
                    {
                        u -= 10;
                        if (u < 26)
                        {
                            return (char)('a' + u);
                        }
                        else
                        {
                            u -= 26;
                            return (char)('A' + u);
                        }
                    }

                case CharKind.Hexadecimal:
                    u = this.GetUInt() % (10 + 6);
                    if (u < 10)
                    {
                        return (char)('0' + u);
                    }
                    else
                    {
                        u -= 10;
                        return (char)('a' + u);
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        private bool ContainsAll(char[] chars, CharKind kind)
        {
            switch (kind)
            {
                case CharKind.NumberAlphabet:
                    if (!chars.Any(a => a >= '0' && a <= '9'))
                    {
                        return false;
                    }
                    else if (!chars.Any(a => a >= 'a' && a <= 'z'))
                    {
                        return false;
                    }
                    else if (!chars.Any(a => a >= 'A' && a <= 'Z'))
                    {
                        return false;
                    }

                    return true;

                case CharKind.Hexadecimal:
                    if (!chars.Any(a => a >= '0' && a <= '9'))
                    {
                        return false;
                    }
                    else if (!chars.Any(a => a >= 'a' && a <= 'f'))
                    {
                        return false;
                    }

                    return true;

                default:
                    throw new InvalidOperationException();
            }
        }

        private uint GetUInt()
        {
            Span<byte> b = stackalloc byte[4];
            this.provider.GetBytes(b);
            return BitConverter.ToUInt32(b);
        }
    }
}
