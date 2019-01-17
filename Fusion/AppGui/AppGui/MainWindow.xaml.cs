using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Kinect;
using System.ComponentModel;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Controls;


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
        private bool iniciou=false;
        private bool player = false;
        
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

        private bool lazer = false;
        private bool acceptingVoiceInput = true;
    
        private bool awaitingVoiceConfirmation = false;
        private bool awaitingVoiceGender = false;
        private bool awaitingConfirmation = false;
        private bool awaitingShutdownConfirmation = false;
        private bool awaitingJarbasExitConfirmation = false;
        private bool awaitingWindowCloseConfirmation = false;
        private bool awaitingStartCommand = true;

        private string swipe_left_lazer_action = "music_player";
        private string swipe_left_lazer_action_string = "ouvir música";
        private string swipe_left_trabalho_action = "email";
        private string swipe_left_trabalho_action_string = "abrir a caixa de correio";
        private string swipe_right_lazer_action = "movies";
        private string swipe_right_lazer_action_string = "ver um filme";
        private string swipe_right_trabalho_action = "browser";
        private string swipe_right_trabalho_action_string = "abrir o browser";

        private string callClient = null;
        private int windowNumber;
        private int screenshot_num = 1;
        private static long timeStamp = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff"));

        private static Action functionToCall = () => {
            if(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff")) > (timeStamp + 2000000))
            {
                warnUserTimeout();
            }
        };

        private System.Threading.Timer timer = new System.Threading.Timer(
            callback => functionToCall(),
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromSeconds(20)
        );

        public MainWindow()
        {
            //InitializeComponent();

            t = new Tts();
            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;

            mmiC.Start();
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
       
        private void sayHello()
        {
            if (awaitingStartCommand)
            {
                awaitingStartCommand = false;
                sayInitialization();
                return;
            }

            rnd = new Random();
            t.Speak(_helloMessages[rnd.Next(0, _helloMessages.Length)]);
        }

        private void sayInitialization()
        {
            t.Speak(Properties.Settings.Default.voiceCallClient +
               ", o meu nome é Jarbas, fui criado pelo Cristiano Vagos e o Gabriel Patrício. " +
               "Sou um controlador do sistema operativo Windows e estou aqui para o ajudar. Por defeito, está no modo de trabalho. Súaipe Esquerda para Email.Súaipe Direita para Navegar");
        }
       
        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            if (!acceptingVoiceInput) return;

            XNamespace emma_ns = "http://www.w3.org/2003/04/emma";
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);

            string raw_confidence;

            if(doc.Descendants(emma_ns + "interpretation").Count() == 1)
            {
                raw_confidence = (string)doc.Descendants(emma_ns + "interpretation").FirstOrDefault().Attribute(emma_ns + "confidence").Value;
            }
            else
            {
                raw_confidence = (string)doc.Descendants(emma_ns + "interpretation").ElementAt(1).Attribute(emma_ns + "confidence").Value;
            }
            
            Console.WriteLine("confidence: " + raw_confidence);
            float confidence = 0F;
            float.TryParse(raw_confidence.Replace(".", ","), out confidence);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);
            onMessage(json, confidence);
        }

        private void onMessage(dynamic json, float confidence)
        {
            string command = (string)json.recognized[0].ToString();
            rnd = new Random();
            bool gesture_command = false;

            switch (command) {
                case "Play_Pause":
                case "Swipe_Right":
                case "Swipe_Left":
                case "Hands_air":
                case "Kill":
                case "Headphones":
                case "Jarbas_init":
                case "screenshot":
                case "lock":
                case "lazer":
                case "trabalho":
                    if(!awaitingStartCommand)
                    {
                        gesture_command = true;
                    }
                    break;
            }

            Console.WriteLine("confidence: " + confidence);
            Console.WriteLine("command: " + command);
            Console.WriteLine("gestureCommand: " + gesture_command);
            Console.WriteLine("awaitingConfirmation: " + awaitingConfirmation);
            Console.WriteLine("awaitingStartCommand: " + awaitingStartCommand);
            Console.WriteLine("awaitingWindowCloseConfirmation: " + awaitingWindowCloseConfirmation);

            if (!gesture_command && confidence >= 0.4 && confidence < 0.6)
            {
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

            if(!(command.Equals("greeting")) && awaitingStartCommand)
            {
                Console.WriteLine("return awaiting start command");
                return;
            }

            makeAction(command, confidence);
        }

        private void makeAction(String command, float confidence)
        {
            switch (command)
            {
                case "Swipe_Right":
                    if (lazer)
                    {
                        makeAction(swipe_right_lazer_action, confidence);
                    }
                    else
                    {
                        makeAction(swipe_right_trabalho_action, confidence);
                    }
                    return;

                case "Swipe_Left":
                    if (lazer)
                    {
                        makeAction(swipe_left_lazer_action, confidence);
                    }
                    else
                    {
                        makeAction(swipe_left_trabalho_action, confidence);
                    }
                    return;

                case "lazer":
                    lazer = true;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Vejo que quer ter o seu momento de lazer" + ".Súaipe Esquerda para "
                        + swipe_left_lazer_action_string + ".Súaipe Direita para " + swipe_right_lazer_action_string);
                    return;

                case "trabalho":
                    lazer = false;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".hora de trabalhar" + ".Súaipe Esquerda para "
                        + swipe_left_trabalho_action_string + ".Súaipe Direita para " + swipe_right_trabalho_action_string);
                    return;

                case "next_track_config":
                case "previous_track_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá mudar músicas com Súaipe Esquerda e Direita no modo lazer");
                    swipe_left_lazer_action = "previous_track";
                    swipe_right_lazer_action = "next_track";
                    swipe_left_lazer_action_string = "Música anterior";
                    swipe_right_lazer_action_string = "Próxima Música";
                    return;

                case "brightness_up_config":
                case "brightness_down_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá mudar o brilho do ecrã com Súaipe Esquerda e Direita no modo lazer");
                    swipe_left_lazer_action = "brightness_down";
                    swipe_right_lazer_action = "brightness_up";
                    swipe_left_lazer_action_string = "Reduzir o brilho do ecrã";
                    swipe_right_lazer_action_string = "Aumentar o brilho do ecrã";
                    return;

                case "email_left_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá aceder ao seu email com Súaipe Esquerda no modo trabalho");
                    swipe_left_trabalho_action = "email";
                    swipe_left_trabalho_action_string = "Aceder ao seu email";
                    return;

                case "email_right_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá aceder ao seu email com Súaipe Direita no modo trabalho");
                    swipe_right_trabalho_action = "email";
                    swipe_right_trabalho_action_string = "Aceder ao seu email";
                    return;

                case "calc_left_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá aceder à calculadora com Súaipe Esquerda no modo trabalho");
                    swipe_left_trabalho_action = "calc";
                    swipe_left_trabalho_action_string = "Aceder à calculadora";
                    return;

                case "calc_right_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá aceder à calculadora com Súaipe Direita no modo trabalho");
                    swipe_right_trabalho_action = "calc";
                    swipe_right_trabalho_action_string = "Aceder à calculadora";
                    return;

                case "website_left_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá aceder ao browser com Súaipe Esquerda no modo trabalho");
                    swipe_left_trabalho_action = "website";
                    swipe_left_trabalho_action_string = "Aceder ao browser";
                    return;

                case "website_right_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá aceder ao browser com Súaipe Direita no modo trabalho");
                    swipe_right_trabalho_action = "website";
                    swipe_right_trabalho_action_string = "Aceder ao browser";
                    return;

                case "meteo_left_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá ver o estado do tempo com Súaipe Esquerda no modo trabalho");
                    swipe_left_trabalho_action = "meteo";
                    swipe_left_trabalho_action_string = "ver o estado do tempo";
                    return;

                case "meteo_right_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá ver o estado do tempo com Súaipe Direita no modo trabalho");
                    swipe_right_trabalho_action = "meteo";
                    swipe_right_trabalho_action_string = "ver o estado do tempo";
                    return;

                case "camera_left_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá ver a sua câmara com Súaipe Esquerda no modo trabalho");
                    swipe_left_trabalho_action = "camera";
                    swipe_left_trabalho_action_string = "ver a sua câmara";
                    return;

                case "camera_right_config":
                    //if (!configMode) return;
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ".Agora poderá ver a sua câmara com Súaipe Direita no modo trabalho");
                    swipe_right_trabalho_action = "camera";
                    swipe_right_trabalho_action_string = "ver a sua câmara";
                    return;

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
                    if (GetBrightness() - 10 > 0)
                    {
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

                case "movies":
                    t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ". A abrir o Netflix...");
                    Process.Start("www.netflix.com");
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

                case "boss":
                case "chefe":
                case "mestre":
                case "professor":
                case "engenheiro":
                    callClient = command;
                    return;

                case "not_ok":
                    if (!awaitingConfirmation)
                    {
                        Console.WriteLine("returning");
                        return;
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
                    else
                    {
                        awaitingConfirmation = false;
                    }

                    return;

                case "ok":
                    if (!awaitingConfirmation)
                    {
                        Console.WriteLine("returning");
                        return;
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
                        if (confidence > 0.8F)
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

        private void LockWorkStation()
        {
            Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
        }

        
        private void sayDevsInformation()
        {
            t.Speak(Properties.Settings.Default.voiceCallClient +
                ", o meu nome é Jarbas, fui criado por dois alunos de Engenharia de Computadores e Telemática, Cristiano Vagos e Gabriel Patrício. " +
                "Sou um controlador do sistema operativo Windows e estou aqui para o ajudar");
        }

        private void startPlayer()
        {
            try
            {
                string finalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Spotify\Spotify.exe";
                Process.Start(finalPath);
                t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ". A abrir o Spótifai...");
            }
            catch (Exception e)
            {
                Process.Start("wmplayer");
                t.Speak(_okMessages[rnd.Next(0, _okMessages.Length)] + Properties.Settings.Default.voiceCallClient
                        + ". A abrir o Windows Media Player...");
            }
        }
        private void nextTrack()
        {
            keybd_event((byte)Keys.MediaNextTrack, 0, 0, 0);
        }

        private void sayJarbasInfo()
        {
            t.Speak(_jarbasInfo[rnd.Next(0, _jarbasInfo.Length)]);
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

        /*
         * [DllImport("User32")]
        private static extern int keybd_event(Byte bVk, Byte bScan, long dwFlags, long dwExtraInfo);
        */

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
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
    }
}
