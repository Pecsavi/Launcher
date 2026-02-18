using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace Launcher.Models
    {
        /// <summary>
        /// Perzisztált bejegyzés a bedobott fájlokhoz.
        /// A DisplayName most nem kötelező (a jelenlegi UI csak a fájlnevet használja),
        /// de fenntartjuk a mezőt, hogy később lehessen átnevezést perzisztálni.
        /// </summary>
        public class DroppedFileEntry
        {
            public string FilePath { get; set; }
            public string DisplayName { get; set; } // jelenleg nem használt; fallback: Path.GetFileName(FilePath)
        }
    }


