using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace MyWebserver
{
    class myweb
    {
        private TcpListener Listentome;
        //private IPAddress IPadress;
        Thread th;

        private int port = 8080;


        public myweb()
        {
            try
            {
                //start listing on the given port




                Listentome = new TcpListener(port);
                Listentome.Start();
                Console.WriteLine("Web Server Running... Press ^C to Stop...");
                //start the thread which calls the method 'StartListen'
                th = new Thread(new ThreadStart(StartListen));
                th.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred while Listening :" + e.ToString());
            }
        }

        /// <summary>
        /// Ist für die Filenameerkennung zuständig
        /// Funktioniert noch nicht
        /// </summary>
        /// <param name="localDirectory"></param>
        /// <returns></returns>
        public string IchmussdieFileskennen(string localDirectory)
        {
            StreamReader sr;
            String wwwfile = "";
            try
            {

                sr = new StreamReader("data\\Default.dat");
                while ((wwwfile = sr.ReadLine()) != null)
                {
                    //Look into the file to find out the default names
                    if (File.Exists(localDirectory + wwwfile) == true)
                    {
                        break;
                        //file gefunden super!!
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred : " + e.ToString());
            }
            if (File.Exists(localDirectory + wwwfile) == true)
            {
                return wwwfile;
            }
            else
            {
                return "";
            }
        }

        public string BrauchedenlokalenPfad(string sMyWebServerRoot, string dirName)
        {
            StreamReader sr;
            String vDirLine = "";
            String vDirName = "";
            String realDir = "";
            int startPos = 0;
            ///Formatierung der Variablen
            ///killt Leerzeichen
            dirName.Trim();
            ///Formatierung zu Kleinschreibung
            ///
            sMyWebServerRoot = sMyWebServerRoot.ToLower();
            ///Formatierung zu Kleinschreibung
            ///
            dirName = dirName.ToLower();

            try
            {
                //Zeilen aus VDir Lesen
                sr = new StreamReader("data\\vDir.Dat");
                while ((vDirLine = sr.ReadLine()) != null)
                {
                    //Zeielnformat nachbessern
                    vDirLine.Trim();
                    if (vDirLine.Length > 0)
                    {
                        startPos = vDirLine.IndexOf(";");
                        vDirLine = vDirLine.ToLower();
                        vDirName = vDirLine.Substring(0, startPos);
                        realDir = vDirLine.Substring(startPos + 1);
                        if (vDirName == dirName)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Es kam zu einem Fehler: " + e.ToString());
            }
            if (vDirName == dirName)
            {
                return realDir;
            }
            else
            {
                return "";
            }

        }

        public string MimeTypefestlegen(string gewünschtesFile)
        {
            StreamReader sr;
            String typeLine = "";
            String mimeType = "";
            String file = "";
            String mime = "";
            //Convertieren des Formats
            gewünschtesFile = gewünschtesFile.ToLower();
            int startPos = gewünschtesFile.Contains(".") ? gewünschtesFile.IndexOf(".") : -1;
            if (startPos > 0)
                file = gewünschtesFile.Substring(startPos);
            else
            {
                Console.WriteLine("empty Input");
                file = "html";
            }
            try
            {
                sr = new StreamReader("data\\Mime.Dat");
                while ((typeLine = sr.ReadLine()) != null)
                {
                    typeLine.Trim();
                    if (typeLine.Length > 0)
                    {
                        startPos = typeLine.IndexOf(";");
                        typeLine = typeLine.ToLower();
                        mime = typeLine.Substring(0, startPos);
                        mimeType = typeLine.Substring(startPos + 1);
                        if (mime == file)
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Es kam zu einem Fehler: " + e.ToString());
            }
            if (mime == file)
                return mimeType;
            else
                return "";
        }

        public void SendHeader(string sHttpVersion, string sMIMEHeader, int iTotBytes, string sStatusCode, ref Socket mySocket)
        {
            String sBuffer = "";
            // if Mime type is not provided set default to text/html
            if (sMIMEHeader.Length == 0)
            {
                sMIMEHeader = "text/html"; // Default Mime Type is text/html
            }
            sBuffer += sHttpVersion + sStatusCode + "\r\n";
            sBuffer += "Server: cx1193719-b\r\n";
            sBuffer += "Content-Type: " + sMIMEHeader + "\r\n";
            sBuffer += "Accept-Ranges: bytes\r\n";
            sBuffer += "Content-Length: " + iTotBytes + "\r\n\r\n";
            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);
            SendToBrowser(bSendData, ref mySocket);
            Console.WriteLine("Total Bytes : " + iTotBytes.ToString());
        }

        public void SendToBrowser(String sData, ref Socket mySocket)
        {
            SendToBrowser(Encoding.ASCII.GetBytes(sData), ref mySocket);
        }

        public void SendToBrowser(Byte[] bSendData, ref Socket mySocket)
        {
            int numBytes = 0; try
            {
                if (mySocket.Connected) 
                { 
                    if ((numBytes = mySocket.Send(bSendData, bSendData.Length, 0)) == -1) 
                        Console.WriteLine("Socket Error cannot Send Packet"); 
                    else 
                    { 
                        Console.WriteLine("No. of bytes send {0}", numBytes); 
                    } 
                }
                else Console.WriteLine("Connection Dropped....");
            }
            catch (Exception e) { Console.WriteLine("Error Occurred : {0} ", e); }
        }

        public void StartListen()
        {
           
            while (true)
            {
                //neue Connection akzeptieren
                Socket mySocket = Listentome.AcceptSocket();
                Console.WriteLine("Socket Type:" + mySocket.SocketType);
                Thread work;
                 if (mySocket.Connected == true)
                 {
                     work = new Thread(() =>Work(mySocket));
                     work.Start();
                 }
            }

        }


        public void Work(Socket mySocket)
        {
            int startPosition = 0;
            String request;
            String dirName;
            String requuestedFile;
            String fehlermeldung;
            String localDir;
            String rootDir = "C:\\MyWebserver\\";
            String physikalPath = "";
            String formatierteMessage = "";
            String antwort = "";
            String get = "";
            String html = "";
            String httpVersion = "";

           
                Console.WriteLine("\nClient Connected!");
                Console.WriteLine("\n==================\n");
                Console.WriteLine("CLient IP {0}\n", mySocket.RemoteEndPoint);
                //Nun erstelle ich ein bytearry und empfange Daten vom client
                Byte[] byteRecive = new Byte[1024];
                int i = mySocket.Receive(byteRecive, byteRecive.Length, 0);
                //Bytearray in string umwandeln
                string buffer = Encoding.ASCII.GetString(byteRecive);
                Console.WriteLine("buffer: " + buffer);
                //zwischen Host: und Connection: die IP Adresse und den Port auslesen
                //ich will nur GET supporten, war nicht einmal gegeben
                if (buffer.Substring(0, 3) == "GET")
                {
                    get = buffer.Substring(4, buffer.IndexOf("HTTP") - 5);
                    if (get == "/")
                    {
                        get = "index.html";
                        StreamReader sr = new StreamReader(rootDir + get);
                        html = sr.ReadToEnd();
                    }
                        Plugin_Sucher _PS = new Plugin_Sucher();
                    Dictionary<string, object> _Plugins = _PS.GetPlugins();
                    foreach (var _dir in _Plugins)
                    {
                        IPlugin.IPlugin _plugin = _dir.Value as IPlugin.IPlugin;
                        if (_plugin.Name == get.Substring(1))
                        {
                            html = _plugin.HTMLText;
                            break;
                        }
                        
                    }
                }
                else
                {
                    Console.WriteLine("Pech gehabt ich supporte nur GET");
                    mySocket.Close();
                }
                httpVersion = buffer.Substring(buffer.IndexOf("HTTP", 1), 8);

                //fileerkennung
                Console.WriteLine("gewünschtes File :" + physikalPath);

                if (File.Exists(physikalPath) == false)
                {
                    fehlermeldung = html;
                    SendHeader(httpVersion, "", fehlermeldung.Length, "404 Not Found", ref mySocket);
                    SendToBrowser(fehlermeldung, ref mySocket);
                    Console.WriteLine(formatierteMessage);
                }
                else
                {
                    int tobytes = 0;
                    antwort = "";

                    FileStream fs = new FileStream(physikalPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    BinaryReader r = new BinaryReader(fs);
                    byte[] bytesa = new byte[fs.Length];
                    int lies;
                    while ((lies = r.Read(bytesa, 0, bytesa.Length)) != 0)
                    {
                        antwort = antwort + Encoding.ASCII.GetString(bytesa, 0, lies);
                        tobytes += lies;
                    }

                    r.Close();
                    fs.Close();

                    SendHeader(httpVersion, "", tobytes, "200 OK", ref mySocket);
                    SendToBrowser(bytesa, ref mySocket);
                }
                mySocket.Close();
            }
        }

    }


