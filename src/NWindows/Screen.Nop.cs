// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

public abstract partial class Screen
{
    /// <summary>
    /// A nop implementation for Screen that does not return any screens or primary screens;
    /// </summary>
    private sealed class NopScreen : Screen
    {
        private static readonly NopScreen Instance = new();

        private NopScreen()
        {
            IsValid = false;
            IsPrimary = false;
            Name = string.Empty;
            Position = default;
            Dpi = default;
            Size = default;
        }

        public override bool IsValid { get; }
        public override bool IsPrimary { get; }
        public override string Name { get; }
        public override Point Position { get; }
        public override Point Dpi { get; }
        public override SizeF Size { get; }

        public class NopScreenManager : IScreenManager
        {
            public Point GetVirtualScreenPosition() => default;

            public Size GetVirtualScreenSize() => default;

            public bool TryUpdateScreens()
            {
                return false;
            }

            public ReadOnlySpan<Screen> GetAllScreens()
            {
                return ReadOnlySpan<Screen>.Empty;
            }

            public Screen? GetPrimaryScreen() => Instance;
        }
    }
}