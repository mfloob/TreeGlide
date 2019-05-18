using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeTree
{
    abstract public class TaskManager
    {
        private List<Task> taskList = new List<Task>();
        public abstract void OnStart();

        public void Add(params Task[] tasks)
        {
            foreach (Task task in tasks)
                taskList.Add(task);
        }

        public void OnTick()
        {
            foreach(Task task in this.taskList)
            {
                if (task.Validate())
                    task.Execute();
            }
        }
    }
}
