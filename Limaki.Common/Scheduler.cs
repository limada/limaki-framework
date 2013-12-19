using System;
using System.Timers;
using System.Diagnostics;

namespace Limaki.Common {

    public class Scheduler : IDisposable {

        public Scheduler() {
            CheckInterval = 60 * 60 * 1000;    //10min
            Task = s => Trace.WriteLine("Scheduler executes task ...");
            Time = DateTime.Now.TimeOfDay.Add(TimeSpan.FromSeconds(10));
            Daily = true;
        }

        /// <summary>
        /// time to start task
        /// </summary>
        public TimeSpan Time { get; set; }

        /// <summary>
        /// timespan in milliseconds when schedular checks for time
        /// </summary>
        public double CheckInterval { get; set; }

        /// <summary>
        /// Task to be executed (async) on time
        /// </summary>
        public Action<Scheduler> Task { get; set; }


        public bool Daily { get; set; }

        public bool Enabled { get; protected set; }

        protected Timer Timer { get; set; }
        public DateTime LastRun { get; protected set; }
        public Func<bool> CheckDay { get; protected set; }

        public DateTime Started { get; protected set; }

        public DateTime NextRun {
            get {
                var now = DateTime.Now;
                if (Daily) {
                    if (LastRun.Equals(default(DateTime)))
                        return Started.Date + Time;
                    else
                        return LastRun.Date.AddDays(1) + Time;

                } else {
                    if (LastRun.Equals(default(DateTime)))
                        return now.Date + Started.TimeOfDay + Time;
                    else
                        return now.Date + LastRun.TimeOfDay + Time;
                }

            }
        }

        public void Start() {
            Stop();
            Started = DateTime.Now;

            Timer = new Timer(this.CheckInterval);
            Timer.Elapsed += (s, e) => {
                var now = DateTime.Now;
                Func<bool> checkDay = () => LastRun.Date < now.Date &&
                                            now.TimeOfDay >= Time;
                if (!Daily)
                    checkDay = () => now.TimeOfDay >= LastRun.TimeOfDay + Time;

                if (now > NextRun) {

                    Timer.Stop();
                    LastRun = DateTime.Now;

                    var task = Task.BeginInvoke(this,ar => { }, "done");
                    Task.EndInvoke(task);

                   
                    Timer.Start();
                }

            };
            Timer.Start();
            Enabled = true;
        }



        public void Stop() {
            if (Timer != null) {
                Timer.Stop();
                Timer.Dispose();
            }
            Timer = null;
            Enabled = false;
        }



        public void Dispose() {
            Stop();
        }


    }
}