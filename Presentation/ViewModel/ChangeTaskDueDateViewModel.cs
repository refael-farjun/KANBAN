using Presentation.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Presentation.ViewModel
{
    class ChangeTaskDueDateViewModel : NotifiableObject
    {
        private BackendController controller;
        private UserModel user;
        
        private TaskModel task;

        private string _taskDueDate;
        public string TaskDueDate { get => _taskDueDate; set { this._taskDueDate = value; RaisePropertyChanged("TaskDueDate"); } }




        public ChangeTaskDueDateViewModel(UserModel user, TaskModel task)
        {
            this.controller = user.Controller;
            this.user = user;
            this.task = task;

            this._taskDueDate = TaskDueDate;

        }

        public void EditTaskDueDate()
        {
            try
            {
                controller.UpdateTaskDueDate(user.Email, task.Creator, task.BoardName, task.ColumnOrdinal, task.TaskID, DateTime.Parse(TaskDueDate));
                RaisePropertyChanged("DueDate");
                MessageBox.Show("Task DueDate Edited Successfully!");

            }
            catch (Exception e)
            {
                MessageBox.Show("Cannot Edit Task DueDate. " + e.Message);

            }
        }
    }
}
