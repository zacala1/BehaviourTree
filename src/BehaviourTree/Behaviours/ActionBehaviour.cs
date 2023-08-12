using System;
using System.Diagnostics;
using System.IO;

namespace BehaviourTree.Behaviours
{
    public sealed class ActionBehaviour<TContext> : BaseBehaviour<TContext>
    {
        private readonly Func<TContext, BehaviourStatus> _action;

        public ActionBehaviour(string name, Func<TContext, BehaviourStatus> action) : base(name)
        {
            _action = action;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
#if DEBUG
            var timer = Stopwatch.StartNew();
#endif
            var status = _action(context);
#if DEBUG
            if (timer.ElapsedMilliseconds >= 100)
            {
                try
                {
                    // TODO 임시 테스트 로그
                    var time = DateTime.Now;
                    var filePath = $"D:\\kctech\\csp\\ControlEngine\\BT_log\\{time:yyyy-MM-dd-HH}.log";
                    Directory.CreateDirectory(@"D:\kctech\csp\ControlEngine\BT_log");
                    using (StreamWriter writer = File.AppendText(filePath))
                    {
                        writer.WriteLine(
                            $"[{time:HH:mm:ss.ffff}] Behavior Node Time. id={Id}, name={Name}, context={typeof(TContext).Name}, status={status}, time={timer.ElapsedMilliseconds}ms");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ffff}] Behavior Log Exception:{e.Message}");
                }
            }
            timer.Stop();
#endif
            return status;
        }
    }
}