using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class TaskService
    {
        private readonly List<TaskItem> _tasks = new();

        private int _nextId = 1;

        public List<TaskItem> GetTasks(string? priority)
        {
            IEnumerable<TaskItem> result = _tasks;

            if (!string.IsNullOrWhiteSpace(priority))
            {
                result = result.Where(task =>
                    task.Priority == priority.ToLowerInvariant()
                );
            }

            return result
                .OrderBy(task => task.Priority switch
                {
                    "high" => 1,
                    "medium" => 2,
                    "low" => 3,
                    _ => 4
                })
                .ToList();
        }

        public TaskItem? GetTask(int id)
        {
            return _tasks.FirstOrDefault(task => task.Id == id);
        }

        public TaskItem CreateTask(string title, string priority)
        {
            TaskItem task = new TaskItem
            {
                Id = _nextId++,
                Title = title,
                Priority = priority.ToLowerInvariant(),
                Status = "new"
            };

            _tasks.Add(task);

            return task;
        }

        public bool UpdateTask(
            int id,
            string title,
            string priority,
            string status)
        {
            TaskItem? task = GetTask(id);

            if (task == null)
            {
                return false;
            }

            task.Title = title;
            task.Priority = priority.ToLowerInvariant();
            task.Status = status.ToLowerInvariant();

            return true;
        }

        public static bool IsValidPriority(string priority)
        {
            return priority == "low"
                || priority == "medium"
                || priority == "high";
        }

        public static bool IsValidStatus(string status)
        {
            return status == "new"
                || status == "completed";
        }
    }
}
