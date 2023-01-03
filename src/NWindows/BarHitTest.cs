// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

public enum BarHitTest
{
    None,
    Menu,
    Help,
    Caption,
    MinimizeButton,
    MaximizeButton,
    CloseButton
    // TODO: We might want to handle the size grip
}