using System;
using System.Windows.Forms;

namespace MatematikOyunu
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int levelToOpen = 1; // Varsayılan olarak seviye 1'den başla

            if (args.Length > 0 && args[0].ToLower() == "open")
            {
                // Hile parametresi var mı kontrol et
                if (args.Length > 1)
                {
                    string levelParam = args[1].ToLower();

                    if (levelParam == "all")
                    {
                        levelToOpen = 7; // Tüm seviyelerin kilidi açık, en yüksek seviyeden başla
                    }
                    else if (int.TryParse(levelParam, out int specifiedLevel) && specifiedLevel >= 1 && specifiedLevel <= 5)
                    {
                        levelToOpen = specifiedLevel; // Seviye 1-5 arası belirtilmişse, bu seviyeye ayarla
                    }
                }
            }

            Form1 gameForm = new Form1(levelToOpen); // Form1 nesnesini başlangıç seviyesiyle başlat
            Application.Run(gameForm);
        }
    }
}

