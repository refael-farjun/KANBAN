﻿using IntroSE.Kanban.Backend.ServiceLayer;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    class Board
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string name;
        public string Name
        {
            get => name;
            private set => name = Name;
        }


        private readonly List<Column> columns; // backlogs , inProgress, done (generic updatable)
        public List<Column> Columns
        {
            get => columns;
        }

        public Board(string name)
        {
            Name = name;
            columns = new List<Column>(3);
            for (int i = 0; i < 3; i++)
                columns[i] = new Column();
            //this.name = name;
            //this.MaxBacklogs = -1;
            //this.MaxInProgress = -1;
            //this.MaxDone = -1;
            //this.columns = new List<Dictionary<int,Task>>();
            //for (int i=0; i<3; i++)
            //{
            //    this.columns.Add(new Dictionary<int, Task>());
            //}
        }

        // pre condition: columnOrdinal < DONE_COLUMN
        public Response AdvanceTask(Task task, int columnOrdinal)
        {
            int taskId = task.ID;
            try
            {
                Columns[columnOrdinal + 1].AddTask(task);
                Columns[columnOrdinal].RemoveTask(task);
            }
            catch(ArgumentException e)
            {
                return new Response(e.Message);
            }
            return new Response();
        }

        public Response LimitColumn(int columnOrdinal, int limit)
        {
            if (columnOrdinal > 2 || columnOrdinal < 0)
                return new Response("there is no such column number");
            try
            {
                Columns[columnOrdinal].MaxTasks = limit;
            }
            catch(ArgumentException e)
            {
                return new Response(e.Message);
            }
            
//                    //return new Response();
//            //if (columnOrdinal == 0)
//            //    MaxBacklogs = limit;
//            //if (columnOrdinal == 1)
//            //    MaxInProgress = limit;
//            //if (columnOrdinal == 2)
//            //    MaxDone = limit;
//            return new Response();
//        }

        public Response<int> GetColumnLimit(int columnOrdinal)
        {
            if (columnOrdinal > 2 || columnOrdinal < 0)
                return Response<int>.FromError("there is no such column number");
            return Response<int>.FromValue(Columns[columnOrdinal].MaxTasks);
        }

        public Response<string> GetColumnName(int columnOrdinal)
        {
            if (columnOrdinal == 0)
                return Response<string>.FromValue("backlog");
            if (columnOrdinal == 1)
                return Response<string>.FromValue("in progress");
            if (columnOrdinal == 2)
                return Response<string>.FromValue("done");
            return Response<string>.FromError("there is no such column number");

//        }

        internal Response<Task> AddTask(DateTime dueDate, string title, string description, string userEmail)
        {
            Task task;
            try
            {
                task = Columns[0].AddTask(dueDate, title, description, userEmail);
            }
            catch(ArgumentException a)
            {
                return Response<Task>.FromError(a.Message);
            }
            return Response<Task>.FromValue(task);
        }

        // pre condition: valid columnOrdinal
        internal Response<IList<Task>> GetColumn(int columnOrdinal)
        {
            return Response<IList<Task>>.FromValue(Columns[columnOrdinal].Tasks);
        }

        //public Response advanceTask(int taskId, int columnOrd)
        //{
        //    if (!containsTask(taskId))
        //        return new Response("Task dose not exist");
        //    if (Columns[2].ContainsKey(taskId))
        //        return new Response("Task is already done");
        //    if (!Columns[columnOrd].ContainsKey(taskId))
        //        return new Response("Task do not exist in this column");
        //    if (Columns[0].ContainsKey(taskId))
        //    {
        //        if (Columns[1].Keys.Count == maxInProgress)
        //            return new Response("Can not advance Task, 'In Progress' column got to its maximum limit");
        //        Task task = Columns[0][taskId];
        //        Columns[0].Remove(taskId);
        //        Columns[1].Add(taskId, task);
        //    }
        //    else if (Columns[1].ContainsKey(taskId))
        //    {
        //        if (Columns[2].Keys.Count == MaxDone)
        //            return new Response("Can not advance Task, 'done' column got to its maximum limit");
        //        Task task = Columns[1][taskId];
        //        Columns[1].Remove(taskId);
        //        Columns[2].Add(taskId, task);
        //    }
        //    return new Response();
        //}

        //public Response<Dictionary<int, Task>> getColumn(int columnOrdinal)
        //{
        //    if (columnOrdinal > 2 || columnOrdinal < 0)
        //        return Response<Dictionary<int, Task>>.FromError("there is no such column number");
        //    return Response<Dictionary<int, Task>>.FromValue(Columns[columnOrdinal]);

        //}


        //private bool containsTask(int taskId)
        //{
        //    bool flag = false;
        //    foreach(Dictionary<int, Task> dict in Columns)
        //    {
        //        if (dict.ContainsKey(taskId))
        //            flag = true;
        //    }
        //    return flag;
        //}

        //public int getNumOfTasks()
        //{
        //    int backlogsTaskNum = Columns[0].Count;
        //    int inProgressTaskNum = Columns[1].Count;
        //    int doneTaskNum = Columns[2].Count;
        //    return backlogsTaskNum + inProgressTaskNum + doneTaskNum;
        //}

        //public Response<Dictionary<int, Task>> getInProgess()
        //{
        //    return Response<Dictionary<int, Task>>.FromValue(Columns[1]);
        //}

        //public Response AddTask(Task task)
        //{
        //    if (Columns[0].Count == MaxBacklogs)
        //        return new Response("Can not add Task, backlogs column got to its maximum limit");
        //    Columns[0].Add(task.ID, task);
        //    return new Response();
        //}


        //private int maxBacklogs;
        //public int MaxBacklogs
        //{
        //    get => maxBacklogs;
        //    set => maxBacklogs = value;
        //}
        //private int maxInProgress;
        //public int MaxInProgress
        //{
        //    get => maxInProgress;
        //    set => maxInProgress = value;
        //}
        //private int maxDone;
        //public int MaxDone
        //{
        //    get => maxDone;
        //    set => maxDone = value;
        //}
    }
}

