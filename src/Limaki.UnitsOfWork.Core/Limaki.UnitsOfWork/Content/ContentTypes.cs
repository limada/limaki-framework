/*
 * Limada
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork.Content {

    public class ContentTypes : GuidFlags {

        public static Guid Unknown { get; } = new Guid ("dd9f6878-c447-47d8-ac71-c02074cce895");

        public static Guid Text { get; } = new Guid ("866fc18a-1525-43f7-a592-e59a9da913ba");
        public static Guid HTML { get; } = new Guid ("cf13b1ce-fe2a-4a7b-b72f-45a344f4793f");
        public static Guid Markdown { get; } = new Guid ("c7825c76-8e11-47ab-9434-4f92602e8316");

        public static Guid ASCII { get; } = new Guid ("70739e6a-4bc9-4ffb-8345-129e5a777437");
        public static Guid RTF { get; } = new Guid ("1e83e58e-e331-48fe-b5a6-57f13ce72395");

        public static Guid PNG { get; } = new Guid ("70c57739-4a6a-4b62-acc1-b5990a690875");
        public static Guid TIF { get; } = new Guid ("dc2c7dd8-0149-464d-a656-11fd4739f8f6");
        public static Guid JPG { get; } = new Guid ("66a90fbd-4c5b-49f1-9202-bcde76edfad8");

        public static Guid GIF { get; } = new Guid ("afc34683-c9bc-4985-8993-e835931184e9");
        public static Guid BMP { get; } = new Guid ("9efe37e3-3624-4d54-8fbf-2bde625a4c35");
        public static Guid DIB { get; } = new Guid ("9b5abd4c-1c06-4dcb-bed1-ae869103fb57");

    }
}
