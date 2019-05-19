using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeGlide
{
    abstract public class TaskManager
    {
        private List<Task> taskList = new List<Task>();
        public abstract void OnStart();

        public async Task<T> Run<T>(T x)
        {
            return await System.Threading.Tasks.Task.Run(() => x);
        }

        public void Add(params Task[] tasks)
        {
            foreach (Task task in tasks)
                taskList.Add(task);
        }

        public async void OnTick()
        {
            foreach(Task task in this.taskList)
            {
                if (await Run(task.Validate()))
                    await Run(task.Execute());
            }
        }
    }
}
