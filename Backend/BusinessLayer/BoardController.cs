﻿using IntroSE.Kanban.Backend.ServiceLayer;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    class BoardController
    {

                        // email creator            boardName 
        private Dictionary<string, Dictionary<string, Board>> boards;
        //             email members,  board name
        private Dictionary<string, HashSet<Board>> members;

        private readonly int DONE_COLUMN;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public BoardController()
        {
            boards = new Dictionary<string, Dictionary<string, Board>>();
            members = new Dictionary<string, HashSet<Board>>();
        }

        /// <summary>        
        /// Checks args validities.
        /// </summary>
        /// <param name="userEmail">The email of the user</param>
        /// <param name="creatorEmail">The email of the creator user</param>
        /// <param name="boardName">The name of the board</param>
        /// <returns>A response<T> object according tho the <c>apply</c> function. The response should contain a error message in case of an error</returns>
        private Response CheckArgs(string userEmail, string creatorEmail, string boardName)
        {
            Response r = isCreator(creatorEmail, boardName);
            if (r.ErrorOccured)
                return r;
            return isMember(userEmail, creatorEmail, boardName);
        }

        /// <summary>        
        /// Checks if 'userEmail' is a member in the board 
        /// Pre condition: there is a board named <c>boardName</c> created by <c>creatorEmail</c>
        /// </summary>
        /// <param name="userEmail">The email of the user</param>
        /// <param name="creatorEmail">The email of the creator user</param>
        /// <param name="boardName">The name of the board</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        private Response isMember(string userEmail, string creatorEmail, string boardName)
        {
            if (!members.ContainsKey(userEmail))
                return new Response($"Could not find {userEmail}");
            if (!members[userEmail].Contains(boards[creatorEmail][boardName]))
                return new Response($"{userEmail} is not a member in this board");
            return new Response();
        }

        /// <summary>        
        /// Checks if the "creatorEmail" is the creator of the board name
        /// </summary>
        /// <param name="creatorEmail">The email of the creator user</param>
        /// <param name="boardName">The name of the board</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        internal Response isCreator(string creatorEmail, string boardName)
        {
            if (!boards.ContainsKey(creatorEmail))
                return new Response($"{creatorEmail} has no boards");

            if (!boards[creatorEmail].ContainsKey(boardName))
                return new Response("There is no board that is named: " + boardName + " that is related to this email: " + creatorEmail);
            
            return new Response();
        }

        /// <summary>        
        /// add new user that registered to the to the members with empty list
        /// </summary>
        /// <param name="userEmail">The email of the user</param>
        internal void addNewUserToMembers(string userEmail)
        {
            members.Add(userEmail, new HashSet<Board>());
        }


        /// <summary>
        /// Limit the number of tasks in a specific column
        /// </summary>
        /// <param name="email">The email address of the user, must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="limit">The new limit value. A value of -1 indicates no limit.</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response LimitColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int limit) 
        {
            Response r = CheckArgs(userEmail, creatorEmail, boardName);
            if (r.ErrorOccured)
                return r;
            if (userEmail != creatorEmail)
                return Response<Object>.FromError("Only creator can limit columns");
            if (columnOrdinal > DONE_COLUMN)
                return Response<Object>.FromError("column ordinal dose not exist. max 2");
            log.Debug($"limit column successfully to {limit}");
            return Response<Object>.FromValue(boards[userEmail][boardName].LimitColumn(columnOrdinal, limit));
        }

        /// <summary>
        /// Get the limit of a specific column
        /// </summary>
        /// <param name="email">The email address of the user, must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <returns>The limit of the column.</returns>
        public Response<int> GetColumnLimit(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            Response r = CheckArgs(userEmail, creatorEmail, boardName);
            if (r.ErrorOccured)
                return Response<int>.FromError(r.ErrorMessage);
            if (!members[userEmail].Contains(boards[creatorEmail][boardName]))
                return Response<int>.FromError("The user is not a board member");
            if (columnOrdinal > DONE_COLUMN)
                return Response<int>.FromError("column ordinal dose not exist. max " + DONE_COLUMN);

            return boards[userEmail][boardName].GetColumnLimit(columnOrdinal);
        }

        /// <summary>
        /// Get the name of a specific column
        /// </summary>
        /// <param name="email">The email address of the user, must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <returns>The name of the column.</returns>
        public Response<string> GetColumnName(string userEmail, string creatorEmail, string boardName, int columnOrdinal) 
        {
            Response r = CheckArgs(userEmail, creatorEmail, boardName);
            if (r.ErrorOccured)
                return Response<string>.FromError(r.ErrorMessage);
            if (!members[userEmail].Contains(boards[creatorEmail][boardName]))
            {
                log.Debug("The user is not a board member");
                return Response<string>.FromError("The user is not a board member");
            }
            if (columnOrdinal > DONE_COLUMN)
                return Response<string>.FromError("column ordinal dose not exist. max " + DONE_COLUMN);
            return boards[userEmail][boardName].GetColumnName(columnOrdinal);
        }

        /// <summary>
        /// Add a new task.
        /// </summary>
        /// <param name="email">Email of the user. The user must be logged in.</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="title">Title of the new task</param>
        /// <param name="description">Description of the new task</param>
        /// <param name="dueDate">The due date if the new task</param>
        /// <returns>A response object with a value set to the Task, instead the response should contain a error message in case of an error</returns>
        public Response<Task> AddTask(string userEmail, string creatorEmail, string boardName, string title, string description, DateTime dueDate)
        {
            Response r = CheckArgs(userEmail, creatorEmail, boardName);
            if (r.ErrorOccured)
                return Response<Task>.FromError(r.ErrorMessage);
            Board b = boards[creatorEmail][boardName];
            Response<Task> task = b.AddTask(dueDate, title, description, userEmail);
            if (!task.ErrorOccured)
                storeTask(userEmail, creatorEmail, boardName, task.Value);
            return task;
        }

        private Response storeTask(string userEmail, string creatorEmail, string boardName, Task value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a task by id from a given specific column and a board name.
        /// </summary>
        /// <param name="email">Email of user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <returns>A response with the value of the task, The response should contain a error message in case of an error</returns>
        private Response<Task> TaskGetter(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId)
        {
            Response r = CheckArgs(userEmail, creatorEmail, boardName);
            if (r.ErrorOccured)
                return Response<Task>.FromError(r.ErrorMessage);
            if (columnOrdinal > DONE_COLUMN || columnOrdinal < 0)
                return Response<Task>.FromError("there is no such column number");
            try
            {
                Task t = boards[creatorEmail][boardName].Columns[columnOrdinal].GetTask(taskId);
                return Response<Task>.FromValue(t);
            }
            catch(ArgumentException e)
            {
                return Response<Task>.FromError($"coldn't find task id {taskId} in email {userEmail} | board {boardName} | column {columnOrdinal}");
            }
            
        }
        
        /// <summary>
        /// Update the due date of a task
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <param name="dueDate">The new due date of the column</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response UpdateTask<T>(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, Func<Task, T> updateFunc) 
        {
            Response r = CheckArgs(userEmail, creatorEmail, boardName);
            if (r.ErrorOccured)
                return r;
            Response<Task> res = TaskGetter(userEmail, creatorEmail, boardName, columnOrdinal, taskId);
            if (res.ErrorOccured)
            {
                log.Error(res.ErrorMessage);
                return res;
            }
            if (userEmail != res.Value.Assignee)
                return new Response("only the assignee of the task can update");
            if (columnOrdinal == DONE_COLUMN)
                return new Response("task that is done, cannot be change");
            try
            {
                updateFunc(res.Value);
            }
            catch(ArgumentException e)
            {
                return new Response(e.Message);
            }
            return new Response();
        }

        public Response UpdateTaskDueDate(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, DateTime dueDate)
        {
            return UpdateTask<DateTime>(userEmail, creatorEmail, boardName, columnOrdinal, taskId, (task) => task.DueDate = dueDate);
        }

        /// <summary>
        /// Update task title
        /// </summary>
        /// <param name="email">Email of user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <param name="title">New title for the task</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response UpdateTaskTitle(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string title) 
        {
            return UpdateTask<string>(userEmail, creatorEmail, boardName, columnOrdinal, taskId, (task) => task.Title = title);
        }

        /// <summary>
        /// Update the description of a task
        /// </summary>
        /// <param name="email">Email of user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <param name="description">New description for the task</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        
        public Response UpdateTaskDescription(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string description) 
        {
            return UpdateTask<string>(userEmail, creatorEmail, boardName, columnOrdinal, taskId, (task) => task.Description = description);
        }

        /// <summary>
        /// Advance a task to the next column
        /// </summary>
        /// <param name="email">Email of user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>

        public Response AdvanceTask(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId) 
        {
            if (columnOrdinal == DONE_COLUMN) // Nitzan: added this condition because 'Board' doesnt have DONE_COLUMN as a magic number.
                return new Response("Cannot advance a task from 'Done' columne");
            Response r = CheckArgs(userEmail, creatorEmail, boardName);
            if (r.ErrorOccured)
                return r;
            Board b = boards[creatorEmail][boardName];
            return UpdateTask<Response>(userEmail, creatorEmail, boardName, columnOrdinal, taskId, (task) => b.AdvanceTask(task, columnOrdinal));
        }

        /// <summary>
        /// Returns a column given it's name
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <returns>A response object with a value set to the Column, The response should contain a error message in case of an error</returns>
        public Response<IList<Task>> GetColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            Response r = CheckArgs(userEmail, creatorEmail, boardName);
            if (r.ErrorOccured)
                return Response<IList<Task>>.FromError(r.ErrorMessage);
            if (columnOrdinal > DONE_COLUMN)
                return Response<IList<Task>>.FromError("column ordinal dose not exist. max 2");
            return boards[userEmail][boardName].GetColumn(columnOrdinal);
        }

        /// <summary>
        /// Adds a board to the specific user.
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in</param>
        /// <param name="name">The name of the new board</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response AddBoard(string email, string name) 
        {
            if (name == null || email == null || name.Length == 0 || email.Length == 0)
                return new Response("null value given");
            if (boards[email].ContainsKey(name))
                return new Response($"user {email} already has board named {name}");
            Board board = new Board(name);
            boards[email].Add(name, board);
            members[email].Add(board);
            return new Response();
        }

        /// <summary>
        /// Removes a board to the specific user.
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in</param>
        /// <param name="name">The name of the board</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response RemoveBoard(string userEmail, string creatorEmail, string boardName)
        {
            if (userEmail != creatorEmail)
            {
                log.Debug("The user is not the board creator");
                return Response<Task>.FromError("The user is not the board creator");
            }

            Response r = CheckArgs(userEmail, creatorEmail, boardName);
            if (r.ErrorOccured)
                return r;

            return RemoveBoardHelper(creatorEmail, boardName);
        }

        private Response RemoveBoardHelper(string creatorEmail, string boardName)
        {
            // TODO: remove all columns and tasks related to this board from the data base (on delete cascade)
            Board board = boards[creatorEmail][boardName];
            boards[creatorEmail].Remove(boardName);
            foreach(KeyValuePair<string, HashSet<Board>> entry in members)
            {
                if (entry.Value.Contains(board))
                    members[entry.Key].Remove(board);
            }
            // delete from data base
            return new Response();
        }

        /// <summary>
        /// Returns all the In progress tasks of the user.
        /// note: 'email' is in members because it was registered.
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in</param>
        /// <returns>A response object with a value set to the list of tasks, The response should contain a error message in case of an error</returns>

        //TODO: complicated for now.. todo after all things fix
        public Response<IList<Task>> InProgressTask(string email) 
        {
           List<Task> tasks = new List<Task>();
            foreach(Board board in members[email])
            {
                Response<IList<Task>> r = board.GetColumn(1); // 1 is the column ordinal of inProgress
                if (r.ErrorOccured)
                    return Response<IList<Task>>.FromError(r.ErrorMessage);
                tasks.AddRange(r.Value.Where((task) => task.Assignee == email).ToList());
            }
            return Response<IList<Task>>.FromValue(tasks);
        }

        /// <summary>
        /// Adds a board created by another user to the logged-in user. 
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the new board</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response JoinBoard(string userEmail, string creatorEmail, string boardName)
        {
            Response r = CheckArgs(userEmail, creatorEmail, boardName);
            if (r.ErrorOccured)
                return r;

            if (members[userEmail].Contains(boards[creatorEmail][boardName]))
            {
                return new Response("The user is already joined to this board");
            }
            members[userEmail].Add(boards[creatorEmail][boardName]);
            return new Response();
        }

        /// <summary>
        /// Assigns a task to a user
        /// Asumption: only task's assignee can assign other board member to the task
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>        
        /// <param name="emailAssignee">Email of the user to assign to task to</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response AssignTask(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string emailAssignee)
        {
            return UpdateTask<string>(userEmail, creatorEmail, boardName, columnOrdinal, taskId, (task) => task.Assignee = emailAssignee);
        }

        /// <summary>
        /// Returns the list of board of a user. The user must be logged-in. The function returns all the board names the user created or joined.
        /// </summary>
        /// <param name="userEmail">The email of the user. Must be logged-in.</param>
        /// <returns>A response object with a value set to the board, instead the response should contain a error message in case of an error</returns>
        public Response<IList<String>> GetBoardNames(string userEmail)
        {
            return Response<IList<String>>.FromValue(members[userEmail].Select((b) => b.Name).ToList());
        }


        //private Response<Task> TaskGetter(string email, string creatorEmail, string boardName, int columnOrdinal, int taskId) // todo - update in the diagram
        //{
        //    Response validArguments = AllBoardsContainsBoardByEmail(email, boardName);
        //    if (validArguments.ErrorOccured)
        //        return Response<Task>.FromError(validArguments.ErrorMessage);
        //    Board b = boards[email][boardName];
        //    Response<Dictionary<int, Task>> res = b.getColumn(columnOrdinal);
        //    if (res.ErrorOccured)
        //        return Response<Task>.FromError(res.ErrorMessage);
        //    Dictionary<int, Task> col = res.Value;
        //    if (!col.ContainsKey(taskId))
        //        return Response<Task>.FromError($"coldn't find task id {taskId} in email {email} | board {boardName} | column {columnOrdinal}");
        //    return Response<Task>.FromValue(col[taskId]);
        //}


        /// <summary>
        /// Check if user has a board in a given name, also inserting a new email address to all boards collections in case its missing
        /// </summary>
        /// <param name="userEmail">The email address of the user, must be logged in</param>
        /// <param name="creatorEmail">The email address of the board's creator user</param>
        /// <param name="boardName">The name of the board</param>
        /// <returns>A response object. The response should contain a error message in case of missing board for user or invalid argments</returns
        // TODO: check if creatorEmail is valid too
        //private Response AllBoardsContainsBoardByEmail(string userEmail, string creatorEmail, string boardName) 
        //{
        //    if (userEmail == null || creatorEmail == null || boardName == null || userEmail.Length == 0 || creatorEmail.Length == 0 || boardName.Length == 0)
        //        return new Response("null value given");
        //    if (!boards.ContainsKey(userEmail)) // TODO: ask Asaf why this is neccessary 
        //        boards.Add(userEmail, new Dictionary<string, Board>());
        //    if (!boards[userEmail].ContainsKey(boardName))
        //        //return Response<bool>.FromError($"user {email} doesn't possess board name {boardName}");
        //        return new Response($"user {email} doesn't possess board name {boardName}");
        //    return new Response();
        //}


    }
}
