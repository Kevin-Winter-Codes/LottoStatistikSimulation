using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LottoSimulator
{
    internal class Program
    {

        private static DateTime WednesdayGame = GetNextWeekday(DayOfWeek.Wednesday);

        private static DateTime SaturdayGame = GetNextWeekday(DayOfWeek.Saturday);

        private static DateTime Today = DateTime.Now;

        private static int? startyear = null;
        private static int? untilYear = null;
        public static IList<LotteryPlayer> PlayerList { get; private set; } = new List<LotteryPlayer>();
        static async Task Main(string[] args)
        {

            //Spieler hinzufügen
            do
            {
                Console.WriteLine("\nLottospieler Erstellen");
                Console.Write("Name: ");
                string name = Console.ReadLine();

                PlayerList.Add(new LotteryPlayer() { Name = name });

                Console.WriteLine();
                Console.WriteLine("Ein weiteren Lottospieler hinzufügen (j) Oder weiter mit beliebige Taste");
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key != ConsoleKey.J)
                    break;


            }
            while (true);


            Console.Clear();
            Console.WriteLine("Startdatum ist Heute: " + DateTime.Now.ToShortDateString());
            Console.WriteLine("Weiter mit Tasteneingabe");
            Console.ReadKey();




            Console.Clear();





            //Tages-Simulator


            while (Today.Year < DateTime.MaxValue.Year)
            {

                //IST HEUTE LOTTO
                if (untilYear.HasValue)
                {
                    if (Today.Year == untilYear.Value)
                    {
                        untilYear = null;
                    }
                }

                if (Today.Date == WednesdayGame.Date || Today.Date == SaturdayGame.Date)
                {
                    LotteryDraw lotteryDraw = new LotteryDraw();
                    lotteryDraw.DrawDate = new DateTime(Today.Year, Today.Month, Today.Day);

                    foreach (LotteryPlayer lotteryPlayer in PlayerList)
                    {
                        if (!untilYear.HasValue)
                        {
                            Console.Clear();
                            if (Today.Date == WednesdayGame.Date)
                            {
                                Console.WriteLine($"Lotterie am Mittwoch ({Today.ToShortDateString()})");
                                Console.WriteLine("#### Weiter mit Keypress ###### ");
                                Console.ReadKey();
                            }

                            if (Today.Date == SaturdayGame.Date)
                            {
                                Console.WriteLine("#### Weiter mit Keypress ###### ");
                                Console.WriteLine($"Lotterie am Samstag ({Today.ToShortDateString()})");
                                Console.ReadKey();
                            }

                            PlayerMenue(lotteryPlayer);
                        }
                    }


                    lotteryDraw.Ziehung();

                    //Auswertung
                    foreach (LotteryPlayer lotteryPlayer in PlayerList)
                    {
                        await lotteryPlayer.PlayDrawRound(lotteryDraw);
                    }

                    if (untilYear.HasValue && startyear.HasValue)
                    {

                        double totalYears = untilYear.Value - startyear.Value;
                        double goneYears = Today.Year - startyear.Value;


                        double percentPerYear = (goneYears * 100) / totalYears;
                        await ConsoleUtility.WriteProgressBar((int)percentPerYear, true);
                    }
                }

                if (Today.Date == WednesdayGame.Date)
                    WednesdayGame = GotoNextWeek(WednesdayGame);

                if (Today.Date == SaturdayGame.Date)
                    SaturdayGame = GotoNextWeek(SaturdayGame);



                //Gehe einen Tag weiter
                Today = Today.AddDays(1);
            }









        }




        private static void PlayerTicketMenü(LotteryPlayer player)
        {
            ConsoleKeyInfo consoleKeyInfo;
            do
            {
                Console.Clear();

                Console.WriteLine($"Player: {player.Name} Ticker - Menü");

                if (player.CurrentLotteryTickets.Count > 0)
                {
                    Console.WriteLine("\n\nVorhandene Tickets");
                    for (int i = 0; i < player.CurrentLotteryTickets.Count; i++)
                    {
                        LotteryTicket ticket = player.CurrentLotteryTickets[i];
                        Console.WriteLine($"Ticket-Nr: {i + 1}");

                        for (int t = 0; t < ticket.LotteryTips.Count; t++)
                        {
                            Console.Write($"Tip.: {t + 1}: ");

                            foreach (int tipNumber in ticket.LotteryTips[t].TipNumbers)
                                Console.Write($" {tipNumber} ");

                            Console.Write("\n");
                        }
                    }
                }

                Console.WriteLine("(N) Ticket hinzufügen");
                Console.WriteLine("(A) AutoTicket hinzufügen");
                Console.WriteLine("(C) Lösche Alle Tickets");
                Console.WriteLine("(B) Back");

                //if (player.CurrentLotteryTickets.Count > 0)
                //{
                //    Console.WriteLine("(D) Ticket löschen");
                //}

                Console.Write("Eingabe > ");
                consoleKeyInfo = Console.ReadKey();
                Console.WriteLine();

                if (consoleKeyInfo.Key == ConsoleKey.D)
                {
                    Console.Write("Geben Sie bitte die Ticket-Nummer ein");

                    int ticketNummer = int.Parse(Console.ReadLine());

                    player.CurrentLotteryTickets.RemoveAt(ticketNummer - 1);

                }

                if (consoleKeyInfo.Key == ConsoleKey.N)
                {
                    NewTicket(player);
                }

                if (consoleKeyInfo.Key == ConsoleKey.A)
                {
                    player.CreateRandomTicket();
                }

                if (consoleKeyInfo.Key == ConsoleKey.C)
                {
                    player.ClearAllTicktes();
                }



            } while (consoleKeyInfo.Key != ConsoleKey.B);

        }

        private static void NewTicket(LotteryPlayer player)
        {
            LotteryTicket ticket = new LotteryTicket();
            ConsoleKeyInfo newTicketKeyInfo;
            do
            {
                if (ticket.LotteryTips.Count == 12)
                    break;

                LotteryTip lotteryTip = new LotteryTip();
                lotteryTip.Superzahl = ticket.SpecialNumber;
                int tipNumber;
                for (int numberIndex = 0; numberIndex < 6; numberIndex++)
                {
                    Console.Clear();

                    Console.Write("Bisher eingegebene Zahlen: ");

                    foreach (int selectedTipNumbers in lotteryTip.TipNumbers)
                    {
                        if (selectedTipNumbers != 0)
                            Console.Write($" {selectedTipNumbers} ");
                    }
                    Console.WriteLine();

                    do
                    {
                        Console.Write("Bitte geben sie Ihre Glückszahl ein > ");
                        tipNumber = int.Parse(Console.ReadLine());
                    }
                    while (lotteryTip.TipNumbers.Contains(tipNumber));

                    lotteryTip.TipNumbers[numberIndex] = tipNumber;
                }

                ticket.LotteryTips.Add(lotteryTip);

                Console.WriteLine();
                Console.WriteLine("Wollen Sie noch ein Lotterie-Feld ausfüllen? (J/N) ");
                newTicketKeyInfo = Console.ReadKey();


            } while (newTicketKeyInfo.Key != ConsoleKey.N);

            player.CurrentLotteryTickets.Add(ticket);
        }
        private static void PlayerMenue(LotteryPlayer player)
        {
            ConsoleKeyInfo consoleKey;

            do
            {
                Console.Clear();
                Console.WriteLine($"Aktuelles Datum: {Today.ToShortDateString()}");
                Console.WriteLine("Name: " + player.Name);
                Console.WriteLine("gespielte Tipps: " + player.PlayedTipps);
                Console.WriteLine("gespielte Lottoscheine " + player.PlayedTickets);
                Console.WriteLine("Gesamt Kosten: " + player.Costs);
                Console.WriteLine("Gesamt Gewinn: " + player.TotalWin);


                Console.WriteLine("----- Gewinnstreuerung -------");
                Console.WriteLine($"6er mit Zusatzzahl: {player.WinsStatistik[ResultType.SechsPlus1]}");
                Console.WriteLine($"6er ohne Zusatzzahl: {player.WinsStatistik[ResultType.SechsOhne1]}");
                Console.WriteLine($"5er mit Zusatzzahl: {player.WinsStatistik[ResultType.FuenfPlus1]}");
                Console.WriteLine($"5er ohne Zusatzzahl: {player.WinsStatistik[ResultType.FuenfOhne1]}");
                Console.WriteLine($"4er mit Zusatzzahl: {player.WinsStatistik[ResultType.VierPlus1]}");
                Console.WriteLine($"4er ohne Zusatzzahl: {player.WinsStatistik[ResultType.VierOhne1]}");
                Console.WriteLine($"3er mit Zusatzzahl: {player.WinsStatistik[ResultType.DreiPlus1]}");
                Console.WriteLine($"3er ohne Zusatzzahl: {player.WinsStatistik[ResultType.DreiOhne1]}");

                Console.WriteLine("\n\n");
                Console.WriteLine("Menüpunkte");
                Console.WriteLine("(1) Lottoscheine verwalten");
                Console.WriteLine("(2) Weiter");
                Console.WriteLine("(3) Weiter bis");

                Console.Write("Eingabe>");
                Console.CursorVisible = true;
                consoleKey = Console.ReadKey();

                if (consoleKey.Key == ConsoleKey.D1 || consoleKey.Key == ConsoleKey.NumPad1)
                {
                    break;
                }
                else if (consoleKey.Key == ConsoleKey.D3 || consoleKey.Key == ConsoleKey.NumPad3)
                {
                    Console.Write("\nBis Anfang welches Jahres? > ");
                    untilYear = int.Parse(Console.ReadLine());
                    startyear = Today.Year;
                    Console.Clear();
                    Console.CursorVisible = false;
                    ConsoleUtility.WriteProgressBar(0);
                    break;
                }
                else if (consoleKey.Key == ConsoleKey.D2 || consoleKey.Key == ConsoleKey.NumPad2)
                {
                    break;
                }

            }
            while (true);


            if (consoleKey.Key == ConsoleKey.D1 || consoleKey.Key == ConsoleKey.NumPad1)
            {
                //Goto PlayerMenü

                PlayerTicketMenü(player);
            }
        }



        static DateTime GetNextWeekday(DayOfWeek day)
        {
            DateTime result = DateTime.Now.AddDays(1);
            while (result.DayOfWeek != day)
                result = result.AddDays(1);
            return result;
        }

        static DateTime GotoNextWeek(DateTime dateTime)
        {
            return dateTime = dateTime.AddDays(7);
        }
    }

    class ConsoleUtility
    {
        const char _block = '■';
        const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
        const string _twirl = "-\\|/";

        public static Task WriteProgressBar(int percent, bool update = false)
        {
            if (update)
                Console.Write(_back);
            Console.Write("[");
            var p = (int)((percent / 10f) + .5f);
            for (var i = 0; i < 10; ++i)
            {
                if (i >= p)
                    Console.Write(' ');
                else
                    Console.Write(_block);
            }
            Console.Write("] {0,3:##0}%", percent);
            return Task.CompletedTask;
        }

        public static void WriteProgress(int progress, bool update = false)
        {
            if (update)
                Console.Write("\b");
            Console.Write(_twirl[progress % _twirl.Length]);
        }
    }

    public class LotteryPlayer
    {

        public LotteryPlayer()
        {
            WinsStatistik.Add(ResultType.ZweiPlus1, 0);
            WinsStatistik.Add(ResultType.DreiOhne1, 0);
            WinsStatistik.Add(ResultType.DreiPlus1, 0);
            WinsStatistik.Add(ResultType.VierOhne1, 0);
            WinsStatistik.Add(ResultType.VierPlus1, 0);
            WinsStatistik.Add(ResultType.FuenfOhne1, 0);
            WinsStatistik.Add(ResultType.FuenfPlus1, 0);
            WinsStatistik.Add(ResultType.SechsOhne1, 0);
            WinsStatistik.Add(ResultType.SechsPlus1, 0);
        }
        public string Name { get; set; }

        public IList<LotteryTicket> CurrentLotteryTickets = new List<LotteryTicket>();

        public int PlayedTickets { get; set; } = 0;

        public int PlayedTipps { get; set; } = 0;

        public double TotalWin { get; set; }
        public double Costs { get; set; }

        public IDictionary<ResultType, int> WinsStatistik { get; set; } = new Dictionary<ResultType, int>();

        public void CreateRandomTicket()
        {
            LotteryTicket lotteryTicket = new();

            for (int i = 0; i < 12; i++)
            {
                LotteryTip lotteryTip = new LotteryTip();
                lotteryTip.Superzahl = lotteryTicket.SpecialNumber;
                for (int index = 0; index < 6; index++)
                {
                    lotteryTip.TipNumbers[index] = new Random().Next(49);
                }

                lotteryTicket.LotteryTips.Add(lotteryTip);
            }
            CurrentLotteryTickets.Add(lotteryTicket);
        }

        public void ClearAllTicktes()
        {
            CurrentLotteryTickets.Clear();
        }
        public async Task PlayDrawRound(LotteryDraw lotteryDraw)
        {

            foreach (LotteryTicket currentTicket in CurrentLotteryTickets)
            {
                LotteryTicket ticketToCheck = currentTicket;

                foreach (LotteryTip tipToCheck in ticketToCheck.LotteryTips)
                {
                    LotteryTip lotteryTip = tipToCheck;
                    await lotteryTip.Check(lotteryDraw.Numbers, lotteryDraw.SpecialNumber);


                    TotalWin += lotteryTip.TipResult.Win;

                    SetWinStatistik(lotteryTip.TipResult.Result);
                    PlayedTipps++;

                }
                if (currentTicket.LotteryTips.Count > 12)
                    throw new Exception();

                if (currentTicket.LotteryTips.Count == 12)
                    Costs += 13.2;
                else
                {
                    Costs += (currentTicket.LotteryTips.Count * 1.2);
                }

                PlayedTickets++;
            }
        }

        private void SetWinStatistik(ResultType resultType)
        {
            if (resultType == ResultType.Nothing)
                return;

            int value = (int)this.WinsStatistik[resultType];

            value++;
            this.WinsStatistik[resultType] = value;
        }
    }

    public class LotteryTip/* : ICloneable*/
    {
        public int TipNr { get; set; } = 0;

        public int[] TipNumbers = new int[6];

        public int Superzahl { get; set; } = 0; //Zahl zwischen 0 und 9
        public LotteryTipResult TipResult { get; set; }


        public Task Check(int[] drawNumbers, int specialNumber)
        {
            bool matchSpecialNumber = false;
            int match = 0;

            foreach (int currentDrawNumber in drawNumbers)
            {
                if (TipNumbers.Contains(currentDrawNumber))
                    match++;
            }

            if (Superzahl == specialNumber)
            {
                match++;
                matchSpecialNumber = true;
            }

            TipResult = new LotteryTipResult();

            if (match == 3)
            {
                if (matchSpecialNumber)
                {
                    TipResult.Result = ResultType.ZweiPlus1;
                }
                else
                {
                    TipResult.Result = ResultType.DreiOhne1;
                }
            }
            else if (match == 4)
            {
                if (matchSpecialNumber)
                {
                    TipResult.Result = ResultType.DreiPlus1;
                }
                else
                {
                    TipResult.Result = ResultType.VierOhne1;
                }
            }
            else if (match == 5)
            {
                if (matchSpecialNumber)
                {
                    TipResult.Result = ResultType.VierPlus1;
                }
                else
                {
                    TipResult.Result = ResultType.FuenfOhne1;
                }
            }
            else if (match == 6)
            {
                if (matchSpecialNumber)
                {
                    TipResult.Result = ResultType.FuenfPlus1;
                }
                else
                {
                    TipResult.Result = ResultType.SechsOhne1;
                }
            }
            else if (match == 7)
            {
                if (matchSpecialNumber)
                {
                    TipResult.Result = ResultType.SechsPlus1;
                }
            }

            return Task.CompletedTask;
        }


        //public object Clone()
        //{
        //    return new LotteryTip() { TipNr = this.TipNr, TipNumbers = this.TipNumbers, TipResult = this.TipResult, Superzahl = this.Superzahl };
        //}
    }
    public class LotteryTicket /*: ICloneable */
    {

        public IList<LotteryTip> LotteryTips { get; set; } = new List<LotteryTip>();
        public int SpecialNumber { get; set; }

        public LotteryTicket()
        {
            SpecialNumber = new Random().Next(9);
        }

        //public object Clone()
        //{
        //    return new LotteryTicket() { LotteryTips = this.LotteryTips, SpecialNumber = this.SpecialNumber };
        //}
    }
    public class LotteryDraw
    {
        public int DrawNr { get; set; } = 0;
        public DateTime DrawDate { get; set; }
        public int[] Numbers = new int[6];
        public int SpecialNumber { get; set; } = 0;


        public void Ziehung()
        {
            int randNumber = 0;


            for (int round = 0; round < 6; round++)
            {
                do
                {
                    randNumber = new Random().Next(49);



                } while (Numbers.Contains(randNumber));

                Numbers[round] = randNumber;
            }

            SpecialNumber = new Random().Next(10);
        }


    }
    public class LotteryTipResult
    {
        private ResultType _result = ResultType.Nothing;

        public int TipNr { get; set; } = 0;

        public double Win { get; set; } = 0;

        public ResultType Result
        {

            get
            {
                return _result;
            }

            set
            {
                _result = value;
                MapWin();
            }
        }

        private double MapWin()
        {
            switch (Result)
            {
                case ResultType.ZweiPlus1:
                    Win = 6;
                    break;
                case ResultType.DreiOhne1:
                    Win = 10.3;
                    break;
                case ResultType.DreiPlus1:
                    Win = 18.2;
                    break;
                case ResultType.VierOhne1:
                    Win = 48.6;
                    break;
                case ResultType.VierPlus1:
                    Win = 168.8;
                    break;
                case ResultType.FuenfOhne1:
                    Win = 5078;
                    break;
                case ResultType.FuenfPlus1:
                    Win = 11777;
                    break;
                case ResultType.SechsOhne1:
                    Win = 1777000;
                    break;
                case ResultType.SechsPlus1:
                    Win = 9750000;
                    break;
                default:
                    Win = 0;
                    break;
            }

            return 0;
        }

        //LotteryTip LotteryTip { get; }
    }




    public enum ResultType { Nothing, ZweiPlus1, DreiOhne1, DreiPlus1, VierOhne1, VierPlus1, FuenfOhne1, FuenfPlus1, SechsOhne1, SechsPlus1 }
}