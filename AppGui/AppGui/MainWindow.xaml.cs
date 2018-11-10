using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;
        private static Tts t;
        private static Random rnd;

        private string[] months = new string[] { "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro" };

        private string[] _okMessages = new string[]
        {
            "Certo",
            "OK",
            "Certíssimo",
            "Recebido",
            "Afirmativo",
            "Muito bem",
        };

        private string[] _jarbasInfo = new string[]
        {
            Properties.Settings.Default.voiceCallClient + ", obrigado por se preocupar comigo. Está tudo bem.",
            Properties.Settings.Default.voiceCallClient + ", eu sou um software. Está sempre tudo bem até haver um bug.",
            "Sinto-me bem " + Properties.Settings.Default.voiceCallClient + ". Obrigado por perguntar.",
            "Está tudo OK " + Properties.Settings.Default.voiceCallClient + "!",
            "O Jarbas está porreiro, " + Properties.Settings.Default.voiceCallClient + ".",
            "Tudo fixe, " + Properties.Settings.Default.voiceCallClient + ". Ainda bem que perguntou.",
            "Se o Cristiano e o Gabriel tiverem boa nota ainda me vou sentir melhor.",
        };

        private string[] _sadMessages = new string[]
        {
            "Peço imensa desculpa, " + Properties.Settings.Default.voiceCallClient,
            "Não fique chateado comigo, " + Properties.Settings.Default.voiceCallClient,
            "O Jarbas assim fica triste...",
            "Eu não quero que me trate assim " + Properties.Settings.Default.voiceCallClient,
            "Desculpe, " + Properties.Settings.Default.voiceCallClient,
            "Estou a ficar triste " + Properties.Settings.Default.voiceCallClient,
        };

        private string[] _notUnderstoodMessages = new string[]
        {
            "Desculpe, " + Properties.Settings.Default.voiceCallClient + ". Não entendi o que disse.",
            Properties.Settings.Default.voiceCallClient + ", pode repetir o que disse?",
            "O que foi que disse, " + Properties.Settings.Default.voiceCallClient + "?",
            Properties.Settings.Default.voiceCallClient + ", não entendi o que disse. Pode repetir?",
        };

        private string[] _thankYouMessages = new string[]
        {
            "Ora essa, " + Properties.Settings.Default.voiceCallClient,
            "De nada, " + Properties.Settings.Default.voiceCallClient,
            "Às suas ordens, " + Properties.Settings.Default.voiceCallClient,
            "Sempre às ordens, " + Properties.Settings.Default.voiceCallClient,
            "Ao seu serviço, " + Properties.Settings.Default.voiceCallClient,
            "Só faço o que você me pede, " + Properties.Settings.Default.voiceCallClient,
            "Estou aqui para si, " + Properties.Settings.Default.voiceCallClient,
            "Estou ao seu dispôr, " + Properties.Settings.Default.voiceCallClient,
        };

        private string[] _helloMessages = new string[]
        {
            "Olá, " + Properties.Settings.Default.voiceCallClient + "!",
            "Olá de novo, " + Properties.Settings.Default.voiceCallClient + "!",
            "Oi, " + Properties.Settings.Default.voiceCallClient + "!",
            "Como é, " + Properties.Settings.Default.voiceCallClient + "?!",
            "Tudo bem, " + Properties.Settings.Default.voiceCallClient + "?",
            "Tudo em ordem, " + Properties.Settings.Default.voiceCallClient + "?",
            "Como estás hoje, " + Properties.Settings.Default.voiceCallClient + "?",
        };

        private static string[] _timeoutMessages = new string[]
        {
            "Olá " + Properties.Settings.Default.voiceCallClient + ", eu ainda estou aqui!",
            Properties.Settings.Default.voiceCallClient + ", esqueceu-se de mim?",
            Properties.Settings.Default.voiceCallClient + ", não se esqueça de mim!",
            "Jarbas chama " + Properties.Settings.Default.voiceCallClient,
            "Alô?! " + Properties.Settings.Default.voiceCallClient + "?! Eu estou aqui...",
            "Cu cu, eu sou o Jarbas! Estou aqui para si!",
        };

        private bool acceptingVoiceInput = true;

        private bool awaitingVoiceConfirmation = false;
        private bool awaitingVoiceGender = false;
        private bool awaitingConfirmation = false;
        private bool awaitingShutdownConfirmation = false;
        private bool awaitingJarbasExitConfirmation = false;
        private bool awaitingWindowCloseConfirmation = false;

        private string callClient = null;
        private int windowNumber;
        private int screenshot_num = 1;
        private static long timeStamp = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff"));

        private static Action functionToCall = () => {
            if(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff")) > (timeStamp + 1000000))
            {
                warnUserTimeout();
            }
        };
        private System.Threading.Timer timer = new System.Threading.Timer(
            callback => functionToCall(),
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromSeconds(20));

        public MainWindow()
        {
            InitializeComponent();

            t = new Tts();
            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;

            init();
        }

        private static void warnUserTimeout()
        {
            rnd = new Random();
            t.Speak(_timeoutMessages[rnd.Next(0, _timeoutMessages.Length)]);
        }

        private void updateTimestamp()
        {
            Console.WriteLine("updating timestamp");
            timeStamp = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        }

        private void init()
        {
            if (!Properties.Settings.Default.configured)
                configureJarbas();
            else
                sayHello();

            mmiC.Start();
        }

        private void sayHello()
        {
            rnd = new Random();
            t.Speak(_helloMessages[rnd.Next(0, _helloMessages.Length)]);
            return;
        }

        private void configureJarbas()
        {
            acceptingVoiceInput = false;

            if(Properties.Settings.Default.firstRun)
            {
                t.Speak("Olá! Eu sou o Jarbas, o seu assistente no Windows. " +
                "Como esta é a minha primeira execução, vou-lhe fazer algumas perguntas. " + 
                "Como é que deseja ser tratado?");
            }
            else
            {
                t.Speak("Vamos dar início à configuração do Jarbas. " +
                "Como é que deseja ser tratado?");
            }

            askForVoiceConfig(false);
        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            if (!acceptingVoiceInput) return;

            XNamespace emma_ns = "http://www.w3.org/2003/04/emma";
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            string raw_confidence = (string)doc.Descendants(emma_ns+"interpretation").FirstOrDefault().Attribute(emma_ns+"confidence").Value;
            Console.WriteLine("confidence: " + raw_confidence);
            float confidence = 0F;
            float.TryParse(raw_confidence.Replace(".", ","), out confidence);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            onMessage(json, confidence);
        }

        private void onMessage(dynamic json, float confidence)
        {
            rnd = new Random();
            string command = (string)json.recognized[0].ToString();

            Console.WriteLine("confidence: " + confidence);
            Console.WriteLine("command: " + command);
            Console.WriteLine("awaitingConfirmation: " + awaitingConfirmation);
            Console.WriteLine("awaitingVoiceConfirmation: " + awaitingVoiceConfirmation);
            Console.WriteLine("awaitingVoiceGender: " + awaitingVoiceGender);
            Console.WriteLine("awaitingWindowCloseConfirmation: " + awaitingWindowCloseConfirmation);

            if (confidence >= 0.6 && confidence < 0.75) {
                t.Speak(_notUnderstoodMessages[rnd.Next(0, _notUnderstoodMessages.Length)]);
                return;
            }
            else
            {
                updateTimestamp();
            }

            if (!(command.Equals("not_ok") || command.Equals("ok")) && awaitingConfirmation)
            {
                Console.WriteLine("return awaitingconfirmation");
                return;
            }

            if (!(command.Equals("boss") || command.Equals("chefe") || command.Equals("mestre") ||
                command.Equals("professor") || command.Equals("engenheiro") || command.Equals("not_ok") || 
                command.Equals("ok")) && awaitingVoiceConfirmation)
            {
                Console.WriteLine("return awaitingVoiceConfirmation");
                return;
            }

            if (!(command.Equals("male") || command.Equals("female")) && awaitingVoiceGender)
            {
                Console.WriteLine("return awaitingVoiceGender");
                return;
            }

            switch (command)
            {
                case "greeting":
                    sayHello();
                    return;

                case "devs":
                    sayDevsInformation();
                    return;

                case "status":
                    sayJarbasInfo();
                    return;

                case "camera":
                    showCamera();
                    return;

                case "screenshot":
                    takeScreenshot();
                    return;

                case "brightness_up":
                    if (GetBrightness() + 10 < 100)
                    {
                        t.Speak("Acabei de aumentar a luminosidade do seu PC. O que acha, " +
                            Properties.Settings.Default.voiceCallClient + "?");
                        SetBrightness((byte)(GetBrightness() + 10));
                    }
                    else
                        t.Speak(Properties.Settings.Default.voiceCallClient + ", o seu ecrã já está no máximo de luminosidade.");
                    return;

                case "brightness_down":
                    if (GetBrightness() - 10 > 0) { 
                        t.Speak("Acabei de baixar a luminosidade do seu PC. O que acha, " +
                            Properties.Settings.Default.voiceCallClient + "?");
                        SetBrightness((byte)(GetBrightness() - 10));
                    }
                    else
                        t.Speak(Properties.Settings.Default.voiceCallClient + ", o seu ecrã já está no mínimo de luminosidade.");
                    return;

                case "obrigado":
                    t.Speak(_thankYouMessages[rnd.Next(0, _thankYouMessages.Length)]);
                    return;

                case "sad":
                    t.Speak(_sadMessages[rnd.Next(0, _sadMessages.Length)]);
                    return;

                case "troll":
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                    player.SoundLocation = "trololo.wav";
                    player.Play();
                    return;

                case "lock":
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient 
                        + ". A bloquear a sessão...");
                    LockWorkStation();
                    return;

                case "desligar":
                    askForShutdownConfirmation();
                    return;

                case "exit":
                    askForJarbasExit();
                    return;

                case "horas":
                    sayHour();
                    return;

                case "data_dia":
                case "data_mes":
                case "data_ano":
                    sayDate(command);
                    return;

                case "calc":
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient 
                        + ". A abrir a calculadora...");
                    Process.Start("calc.exe");
                    return;

                case "meteo":
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ". A abrir a meteorologia...");
                    Process.Start(@"msnweather://forecast?la=40.6442700&lo=-8.6455400");
                    return;

                case "email":
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ". A abrir a sua caixa de correio...");
                    Process.Start("mailto:cristianovagos@ua.pt,gpatricio@ua.pt?Subject=O Jarbas envia mails&Body=Queremos ter boa nota, professor!");
                    return;

                case "browser":
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ". A abrir o seu browser...");
                    Process.Start("www.ua.pt");
                    return;

                case "iniciar":
                    startMenuToggle();
                    return;

                case "bateria":
                    sayBatteryInfo();
                    return;

                case "volume_up":
                    increaseVolume();
                    return;

                case "volume_down":
                    decreaseVolume();
                    return;

                case "volume_mute":
                    muteVolume();
                    return;

                case "music_player":
                    startPlayer();
                    return;

                case "play_pause":
                    playPauseTrack();
                    return;

                case "stop_track":
                    stopTrack();
                    return;

                case "next_track":
                    nextTrack();
                    return;

                case "previous_track":
                    previousTrack();
                    return;

                case "config":
                    configureJarbas();
                    return;

                case "male":
                case "female":
                    onVoiceGenderConfigReceived(command);
                    return;

                case "boss":
                case "chefe":
                case "mestre":
                case "professor":
                case "engenheiro":
                    callClient = command;
                    onVoiceConfigReceived();
                    return;

                case "not_ok":
                    if (!awaitingConfirmation)
                    {
                        Console.WriteLine("returning");
                        return;
                    }
                    else if (awaitingVoiceConfirmation)
                    {
                        Console.WriteLine("not ok, calling askforvoiceconfig");
                        askForVoiceConfig(true);
                    }
                    else if (awaitingShutdownConfirmation)
                    {
                        Console.WriteLine("not ok, awaiting shutdown confirmation");
                        awaitingShutdownConfirmation = false;
                    }
                    else if (awaitingWindowCloseConfirmation)
                    {
                        Console.WriteLine("not ok, awaiting window close confirmation");
                        awaitingWindowCloseConfirmation = false;
                    }
                    else if (awaitingJarbasExitConfirmation)
                    {
                        Console.WriteLine("not ok, awaiting jarbas exit confirmation");
                        awaitingJarbasExitConfirmation = false;
                    }

                    return;

                case "ok":
                    if (!awaitingConfirmation) {
                        Console.WriteLine("returning");
                        return;
                    }
                    else if (awaitingVoiceConfirmation)
                    {
                        Console.WriteLine("ok, calling onvoiceconfigconfirmation");
                        onVoiceConfigConfirmation();
                    }
                    else if (awaitingShutdownConfirmation)
                    {
                        if (confidence > 0.9F)
                        {
                            Console.WriteLine("ok, shutdownPC");
                            shutdownPC();
                        }
                        else
                        {
                            t.Speak(Properties.Settings.Default.voiceCallClient 
                                + ", tomei a liberdade de cancelar o seu pedido de encerramento do PC. " +
                                "Por favor, faça o pedido novamente.");
                            awaitingShutdownConfirmation = false;
                            awaitingConfirmation = false;
                        }
                    }
                    else if (awaitingWindowCloseConfirmation)
                    {
                        if(confidence > 0.8F)
                        {
                            Console.WriteLine("ok, closing window");
                            closeWindow(windowNumber);
                        }
                        else
                        {
                            t.Speak(Properties.Settings.Default.voiceCallClient
                                + ", tomei a liberdade de cancelar o seu pedido de fecho da " + windowNumber + "ª janela. "
                                + "Por favor, faça o pedido novamente.");
                            awaitingWindowCloseConfirmation = false;
                            awaitingConfirmation = false;
                        }
                    }
                    else if (awaitingJarbasExitConfirmation)
                    {
                        if (confidence > 0.9F)
                        {
                            Console.WriteLine("ok, exiting Jarbas");
                            t.Speak("Xau, " + Properties.Settings.Default.voiceCallClient + ". Vou descansar um pouco.");
                            Environment.Exit(0);
                        }
                        else
                        {
                            t.Speak(Properties.Settings.Default.voiceCallClient
                                + ", tomei a liberdade de cancelar o seu pedido de fecho do Jarbas. "
                                + "Por favor, faça o pedido novamente.");
                            awaitingWindowCloseConfirmation = false;
                            awaitingConfirmation = false;
                        }
                    }

                    return;

                case "cancel":
                    acceptingVoiceInput = true;
                    awaitingConfirmation = false;
                    awaitingVoiceConfirmation = false;
                    awaitingVoiceGender = false;
                    awaitingShutdownConfirmation = false;
                    awaitingWindowCloseConfirmation = false;
                    return;
            }

            if (command.Contains("close_window_"))
            {
                windowNumber = int.Parse(command.Replace("close_window_", ""));
                askForWindowCloseConfirmation(windowNumber);
            }
            else if (command.Contains("max_window_"))
            {
                windowNumber = int.Parse(command.Replace("max_window_", ""));
                maximizeWindow(windowNumber);
                t.Speak(Properties.Settings.Default.voiceCallClient + ", acabei de maximizar a " 
                    + windowNumber + "ª janela.");
            }
            else if (command.Contains("min_window_"))
            {
                windowNumber = int.Parse(command.Replace("min_window_", ""));
                minimizeWindow(windowNumber);
                t.Speak(Properties.Settings.Default.voiceCallClient + ", acabei de minimizar a "
                    + windowNumber + "ª janela.");
            }
        }

        private void sayDevsInformation()
        {
            t.Speak(Properties.Settings.Default.voiceCallClient +
                ", eu fui criado por dois alunos de Engenharia de Computadores e Telemática, Cristiano Vagos e Gabriel Patrício. " +
                "Devia pagar-lhes uns finos!");
        }

        private void sayJarbasInfo()
        {
            t.Speak(_jarbasInfo[rnd.Next(0, _jarbasInfo.Length)]);
        }

        private void startPlayer()
        {
            try
            {
                string finalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Spotify\Spotify.exe";
                Process.Start(finalPath);
                t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ". A abrir o Spotify...");
            }
            catch (Exception e)
            {
                Process.Start("wmplayer");
                t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ". A abrir o Windows Media Player...");
            }
        }

        private void increaseVolume()
        {
            for (int i = 0; i < 10; i++)
            {
                keybd_event((byte)Keys.VolumeUp, 0, 0, 0);
            }
        }

        private void decreaseVolume()
        {
            for (int i = 0; i < 10; i++)
            {
                keybd_event((byte)Keys.VolumeDown, 0, 0, 0);
            }
        }

        private void muteVolume()
        {
            keybd_event((byte)Keys.VolumeMute, 0, 0, 0);
        }

        private void nextTrack()
        {
            keybd_event((byte)Keys.MediaNextTrack, 0, 0, 0);
        }

        private void previousTrack()
        {
            for (int i = 0; i < 2; i++)
            {
                keybd_event((byte)Keys.MediaPreviousTrack, 0, 0, 0);
            }
        }

        private void playPauseTrack()
        {
            keybd_event((byte)Keys.MediaPlayPause, 0, 0, 0);
        }

        private void stopTrack()
        {
            keybd_event((byte)Keys.MediaStop, 0, 0, 0);
        }

        private void showCamera()
        {
            Process.Start("microsoft.windows.camera:");
            t.Speak(Properties.Settings.Default.voiceCallClient + ", olhe o passarinho...");
        }

        private void takeScreenshot()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (var gfxScreenshot = Graphics.FromImage(bmpScreenshot))
            {
                gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);
            }

            bmpScreenshot.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + 
                @"\screenshot" + screenshot_num + ".jpg", ImageFormat.Jpeg);
            t.Speak(Properties.Settings.Default.voiceCallClient + ", acabei de lhe tirar um printscreen. " +
                "Está disponível no ambiente de trabalho.");
            screenshot_num += 1;
        }

        private void askForWindowCloseConfirmation(int number)
        {
            acceptingVoiceInput = false;

            t.Speak(Properties.Settings.Default.voiceCallClient + ", tem a certeza que pretende fechar a " + number + "ª janela?");

            acceptingVoiceInput = true;
            awaitingConfirmation = true;
            awaitingWindowCloseConfirmation = true;
        }

        private void closeWindow(int number)
        {
            Process[] processList = Array.FindAll(Process.GetProcesses(), process => !String.IsNullOrEmpty(process.MainWindowTitle));
            if (!processList[number - 1].HasExited)
                processList[number - 1].CloseMainWindow();
        }

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private void minimizeWindow(int number)
        {
            int MINIMIZE = 6;

            Process[] processList = Array.FindAll(Process.GetProcesses(), process => !String.IsNullOrEmpty(process.MainWindowTitle));
            ShowWindow(processList[number - 1].MainWindowHandle, MINIMIZE);
        }

        private void maximizeWindow(int number)
        {
            int MAXIMIZE = 3;

            Process[] processList = Array.FindAll(Process.GetProcesses(), process => !String.IsNullOrEmpty(process.MainWindowTitle));
            ShowWindow(processList[number - 1].MainWindowHandle, MAXIMIZE);
        }

        private int GetBrightness()
        {
            System.Management.ManagementScope s = new System.Management.ManagementScope("root\\WMI");
            System.Management.SelectQuery q = new System.Management.SelectQuery("WmiMonitorBrightness");

            System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher(s, q);
            System.Management.ManagementObjectCollection moc = mos.Get();

            byte curBrightness = 0;
            foreach (System.Management.ManagementObject o in moc)
            {
                curBrightness = (byte)o.GetPropertyValue("CurrentBrightness");
                break;
            }

            moc.Dispose();
            mos.Dispose();

            return (int)curBrightness;
        }

        private void SetBrightness(byte targetBrightness)
        {
            System.Management.ManagementScope s = new System.Management.ManagementScope("root\\WMI");
            System.Management.SelectQuery q = new System.Management.SelectQuery("WmiMonitorBrightnessMethods");

            System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher(s, q);
            System.Management.ManagementObjectCollection moc = mos.Get();

            foreach (System.Management.ManagementObject o in moc)
            {
                o.InvokeMethod("WmiSetBrightness", new Object[] { UInt32.MaxValue, targetBrightness });
                break;
            }

            moc.Dispose();
            mos.Dispose();
        }

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        private void shutdownPC()
        {
            t.Speak("Adeus " + Properties.Settings.Default.voiceCallClient + ", até à próxima!");

            awaitingShutdownConfirmation = false;
            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        private void askForShutdownConfirmation()
        {
            acceptingVoiceInput = false;

            t.Speak(Properties.Settings.Default.voiceCallClient + ", tem a certeza que pretende desligar o computador?");

            acceptingVoiceInput = true;
            awaitingConfirmation = true;
            awaitingShutdownConfirmation = true;
        }

        private void askForJarbasExit()
        {
            acceptingVoiceInput = false;

            t.Speak(Properties.Settings.Default.voiceCallClient + ", tem a certeza que me quer desligar?");

            acceptingVoiceInput = true;
            awaitingConfirmation = true;
            awaitingJarbasExitConfirmation = true;
        }

        private void sayHour()
        {
            t.Speak(Properties.Settings.Default.voiceCallClient + ", são " + DateTime.Now.Hour + " horas e " + DateTime.Now.Minute + 
                " minutos.");
        }

        private void sayDate(string command)
        {
            string phrase = Properties.Settings.Default.voiceCallClient;

            switch (command)
            {
                case "data_dia":
                    phrase += " , hoje é dia " + DateTime.Now.Day + " de " + months[DateTime.Now.Month - 1] + " de " + DateTime.Now.Year;
                    break;
                case "data_mes":
                    phrase += " , estamos no mês de " + months[DateTime.Now.Month-1];
                    break;
                case "data_ano":
                    phrase += " , estamos no ano " + DateTime.Now.Year;
                    break;
            }
            t.Speak(phrase);
        }

        [DllImport("User32")]
        private static extern int keybd_event(Byte bVk, Byte bScan, long dwFlags, long dwExtraInfo);
        private const byte UP = 2;
        private const byte CTRL = 17;
        private const byte ESC = 27;

        private void startMenuToggle()
        {
            t.Speak(Properties.Settings.Default.voiceCallClient + ", aqui está o menu de Start do Windows");

            // Press Ctrl-Esc key to open Start menu
            keybd_event(CTRL, 0, 0, 0);
            keybd_event(ESC, 0, 0, 0);

            // Need to Release those two keys
            keybd_event(CTRL, 0, UP, 0);
            keybd_event(ESC, 0, UP, 0);
        }

        private void sayBatteryInfo()
        {
            PowerStatus p = SystemInformation.PowerStatus;
            int a = (int)(p.BatteryLifePercent * 100);
            if (a < 25)
                t.Speak(Properties.Settings.Default.voiceCallClient + ", tem neste momento " + a + "% de bateria. " +
                    "Se quer a minha opinião, penso que será uma boa ideia ligar o seu computador à corrente.");
            else
                t.Speak(Properties.Settings.Default.voiceCallClient + ", tem neste momento " + a + "% de bateria.");
        }

        private void askForVoiceConfig(bool config)
        {
            acceptingVoiceInput = false;

            if (config)
                t.Speak("Como é que deseja ser tratado?");

            acceptingVoiceInput = true;
            awaitingVoiceConfirmation = true;
        }

        private void onVoiceConfigReceived()
        {
            acceptingVoiceInput = false;

            t.Speak("OK " + callClient + ", a partir de agora irei tratá-lo assim. " +
                "Tem a certeza que quer este tipo de tratamento?");

            acceptingVoiceInput = true;
            awaitingConfirmation = true;
        }

        private void onVoiceConfigConfirmation()
        {
            acceptingVoiceInput = false;

            t.Speak("OK " + callClient + ", configuração guardada. E agora, que género prefere para a minha voz?");

            awaitingConfirmation = false;
            awaitingVoiceConfirmation = false;
            Properties.Settings.Default.voiceCallClient = callClient;
            Properties.Settings.Default.Save();

            acceptingVoiceInput = true;
            awaitingVoiceGender = true;
        }

        private void onVoiceGenderConfigReceived(string gender)
        {
            if (gender.Equals("male"))
                t.changeVoiceGender(true);
            else
                t.changeVoiceGender(false);

            t.Speak("Género da voz seleccionado com sucesso. Muito bem " + Properties.Settings.Default.voiceCallClient 
                + ", a configuração do assistente está completa.");

            Properties.Settings.Default.voiceGender = gender;
            Properties.Settings.Default.configured = true;
            Properties.Settings.Default.firstRun = false;
            Properties.Settings.Default.Save();

            awaitingVoiceGender = false;
        }
    }
}
