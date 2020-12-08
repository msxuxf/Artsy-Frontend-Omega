using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 public static class Materials
 {
     static string[] paletteTitles = 
     {
        "Basic",
        "Mystic Sea",
        "Sunset",
        "Fairy Tale",
        "Skater Boy",
        "Summer",
        "Bold, Brave",
        "Go Blue",
        "Skin Tones"
    };

    static int[, ,] rgbs = new int[,,] {
        { {255, 0, 0}, {255, 255, 0}, {0, 0, 255}, {0, 255, 0}, {255, 0, 255} },
        { {38, 83, 43}, {57, 158, 90}, {90, 188, 185}, {99, 226, 198}, {110, 249, 245} },
        { {53, 80, 112}, {109, 89, 122}, {181, 101, 118}, {229, 107, 111}, {234, 172, 139} },
        { {78, 255, 239}, {115, 166, 173}, {155, 151, 178}, {216, 167, 202}, {199, 184, 234} },
        { {255, 231, 76}, {255, 89, 100}, {220, 220, 220}, {56, 97, 140}, {53, 167, 255} },
        { {239, 71, 111}, {255, 209, 102}, {6, 214, 160}, {17, 138, 178}, {7, 59, 76} },
        { {255, 190, 11}, {251, 86, 7}, {255, 0, 110}, {131, 56, 236}, {58, 134, 255} },
        { {0, 41, 107}, {0, 63, 136}, {0, 80, 157}, {253, 197, 0}, {255, 213, 0} },
        { {107, 63, 52}, {204, 156, 128}, {229, 175, 144}, {255, 195, 160}, {255, 217, 194} }
    };
    
    static string[] brushTitles =
    {
        "Basic",
        "Acrylic",
        "Aurora",
        "Charcoal",
        "Faded",
        "Fresco",
        "Sharpened",
        "Snow",
        "Tarkine",
        "Thylacine",
        "Watercolor",
    };
 }