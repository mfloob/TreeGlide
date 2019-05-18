using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeTree
{
    public class GrindBot : TaskManager
    {
        public override void OnStart()
        {
            Add(new AttackTarget(), new MoveToNearestEnemy(), new StuckCheck());
        }
    }
}
