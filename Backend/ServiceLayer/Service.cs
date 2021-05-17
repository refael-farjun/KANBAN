﻿using System.Collections.Generic;
using System;
using System.Linq;
using IntroSE.Kanban.Backend.BusinessLayer;
using log4net;
using System.Reflection;
using log4net.Config;
using System.IO;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class Service
    {
        private UserController userController;
        private BoardController boardController;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Service()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            log.Info("Starting up!");

            userController = new UserController();
            boardController = new BoardController();
        }

        ///<summary>This method loads the data from the persistance.
        ///         You should call this function when the program starts. </summary>
        public Response LoadData()
        {
            throw new NotImplementedException();
        }
        ///<summary>Removes all persistent data.</summary>
        public Response DeleteData()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="userEmail">The email address of the user to register</param>
        /// <param name="password">The password of the user to register</param>
        /// <returns>A response object. The response should contain a error message in case of an error<returns>
        public Response Register(string userEmail, string password)
        {
            log.Info($"User {userEmail} is trying to Register");
            Response r = userController.Register(userEmail, password);
            if (r.ErrorOccured)
            {
                log.Debug(r.ErrorMessage);
                return r;
            }
            boardController.addNewUserToMembers(userEmail);
            return r;
        }

        //private Response checkArgs(string userEmail, string creatorEmail, string boardName)
        //{
        //    Response r = IsLoggedIn(userEmail);
        //    if (r.ErrorOccured)
        //        return r;
        //    r = emailExist(creatorEmail);
        //    if (r.ErrorOccured)
        //        return r;
        //    r = isCreator(creatorEmail, boardName);
        //    if (r.ErrorOccured)
        //        return r;
        //    return r;
        //}

        /// Log in an existing user
        /// </summary>
        /// <param name="userEmail">The email address of the user to login</param>
        /// <param name="password">The password of the user to login</param>
        /// <returns>A response object with a value set to the user, instead the response should contain a error message in case of an error</returns>
        public Response<User> Login(string userEmail, string password)
        {
            log.Info($"User {userEmail} is trying to Login");
            Response r = userController.Login(userEmail, password);
            if (r.ErrorOccured)
                return Response<User>.FromError(r.ErrorMessage);
            return Response<User>.FromValue(new User(userEmail));
        }
        /// <summary>        
        /// Log out an logged-in user. 
        /// </summary>
        /// <param name="userEmail">The email of the user to log out</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response Logout(string userEmail)
        {
            log.Info($"User {userEmail} is trying to Logout");
            return userController.Logout(userEmail);
        }

        /// <summary>        
        /// Checks if a user is logged in
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        private Response IsLoggedIn(string email)
        {
            Response<bool> r = userController.isLoggedIn(email);
            if (r.ErrorOccured)
            {
                log.Debug(r.ErrorMessage);
                return r;
            }
                
            if (!r.Value)
            {
                string msg = $"User {email} is not logged in";
                log.Debug(msg);
                return new Response(msg);
            }
                
            return new Response();
        }

        private Response<T> IsLoggedInAndApply<T>(string email, Func<Response<T>> func)
        {
            Response r = IsLoggedIn(email);
            if (r.ErrorOccured)
                return Response<T>.FromError(r.ErrorMessage);
            return func();
        }

        //private Response emailExist(string email)
        //{
        //    return userController.containsEmail(email);
        //}

        /// <summary>        
        /// Checks if the "creatorEmail" is the creator of the board name
        /// </summary>
        /// <param name="creatorEmail">The email of the creator user</param>
        /// <param name="boardName">The name of the board</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        //private Response isCreator(string creatorEmail, string boardName)
        //{
        //    return boardController.isCreator(creatorEmail, boardName);
        //}

        /// <summary>
        /// Limit the number of tasks in a specific column
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="limit">The new limit value. A value of -1 indicates no limit.</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response LimitColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int limit)
        {
            log.Info($"User {userEmail} is trying to LimitColumn in board {boardName}, column {columnOrdinal} with limit: {limit}");
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return r;
            return boardController.LimitColumn(userEmail, creatorEmail, boardName, columnOrdinal, limit);// TODO: this func just the creator can do
        }

        /// <summary>
        /// Get the limit of a specific column
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <returns>The limit of the column.</returns>
        public Response<int> GetColumnLimit(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {

            log.Info($"User {userEmail} is trying to GetColumnLimit in board {boardName}, column {columnOrdinal}");
            return boardController.GetColumnLimit(userEmail, creatorEmail, boardName, columnOrdinal); // TODO this func evry one can do
        }

        /// <summary>
        /// Get the name of a specific column
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <returns>The name of the column.</returns>
        public Response<string> GetColumnName(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            log.Info($"User {userEmail} is trying to GetColumnName in board {boardName}, column {columnOrdinal}");
            return boardController.GetColumnName(userEmail, creatorEmail, boardName, columnOrdinal);// TODO every one cad do this
        }

        /// <summary>
        /// Add a new task.
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="title">Title of the new task</param>
        /// <param name="description">Description of the new task</param>
        /// <param name="dueDate">The due date if the new task</param>
        /// <returns>A response object with a value set to the Task, instead the response should contain a error message in case of an error</returns>
        public Response<Task> AddTask(string userEmail, string creatorEmail, string boardName, string title, string description, DateTime dueDate)
        {
            log.Info($"User {userEmail} is trying to AddTask: {boardName}, {title}, {description}, {dueDate}");

            Response<BusinessLayer.Task> rT = boardController.AddTask(userEmail, creatorEmail, boardName, title, description, dueDate); // TODO all board memeber can addTAsk
            if (rT.ErrorOccured)
                return Response<Task>.FromError(rT.ErrorMessage);
            BusinessLayer.Task task = rT.Value;
            log.Debug($"task {title} added successfully to board {boardName}");
            return Response<Task>.FromValue(new Task(task.ID, task.CreationTime, task.Title, task.Description, task.DueDate, task.Assignee)); // TODO change to the new Task they gave us
        }


        /// <summary>
        /// Update the due date of a task
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <param name="dueDate">The new due date of the column</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response UpdateTaskDueDate(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, DateTime dueDate)
        {
            log.Info($"User {userEmail} is trying to UpdateTaskDueDate");
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return r;
            return boardController.UpdateTaskDueDate(userEmail, creatorEmail, boardName, columnOrdinal, taskId, dueDate); // TODO CHECK: only assginee can update
        }
        /// <summary>
        /// Update task title
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <param name="title">New title for the task</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response UpdateTaskTitle(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string title)
        {
            log.Info($"User {userEmail} is trying to UpdateTaskTitle");
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return r;
            return boardController.UpdateTaskTitle(userEmail, creatorEmail, boardName, columnOrdinal, taskId, title);  // TODO CHECK: only assginee can update
        }
        /// <summary>
        /// Update the description of a task
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <param name="description">New description for the task</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response UpdateTaskDescription(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId, string description)
        {
            log.Info($"User {userEmail} is trying to UpdateTaskDescription");
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return r;
            return boardController.UpdateTaskDescription(userEmail, creatorEmail, boardName, columnOrdinal, taskId, description);  // TODO CHECK: only assginee can update
        }
        /// <summary>
        /// Advance a task to the next column
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response AdvanceTask(string userEmail, string creatorEmail, string boardName, int columnOrdinal, int taskId)
        {
            log.Info($"User {userEmail} is trying to AdvanceTask in board {boardName}, column {columnOrdinal}, task {taskId}");
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return r;
            return boardController.AdvanceTask(userEmail, creatorEmail, boardName, columnOrdinal, taskId); // TODO CHECK: only assginee can advence
        }
        /// <summary>
        /// Returns a column given it's name
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <returns>A response object with a value set to the Column, The response should contain a error message in case of an error</returns>
        public Response<IList<Task>> GetColumn(string userEmail, string creatorEmail, string boardName, int columnOrdinal)
        {
            log.Info($"User {userEmail} is trying to GetColumn: {boardName}, {columnOrdinal}");
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return Response<IList<Task>>.FromError(r.ErrorMessage);
            Response<IList<BusinessLayer.Task>> returned = boardController.GetColumn(userEmail, creatorEmail, boardName,columnOrdinal); // TODO evryone of the members can apply
            if (returned.ErrorOccured)
            {
                log.Debug(returned.ErrorMessage);
                return Response<IList<Task>>.FromError(returned.ErrorMessage);
            }
            return ConvertBusinessToServiceTasksCollection(returned.Value);
        }
        /// <summary>
        /// Creates a new board for the logged-in user.
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="boardName">The name of the new board</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response AddBoard(string userEmail, string boardName)
        {
            log.Info($"User {userEmail} is trying to AddBoard: {boardName}");
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return r;
            r = boardController.AddBoard(userEmail, boardName);
            if (r.ErrorOccured)
                log.Error(r.ErrorMessage);
            log.Info($"Borad for user {userEmail} succesfully added");
            return r;

        }
        /// <summary>
        /// Removes a board.
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response RemoveBoard(string userEmail, string creatorEmail, string boardName)
        {
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return r;
            return boardController.RemoveBoard(userEmail, creatorEmail, boardName);// TODO: ONLY creator can removed
        }
        /// <summary>
        /// Returns all the in-progress tasks of the logged-in user is assigned to.
        /// </summary>
        /// <param name="userEmail">Email of the logged in user</param>
        /// <returns>A response object with a value set to the list of tasks, The response should contain a error message in case of an error</returns>
        public Response<IList<Task>> InProgressTasks(string userEmail)
        {
            log.Info($"{userEmail} is Trying to get InProgressTasks");
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return Response<IList<Task>>.FromError(r.ErrorMessage);
            Response<IList<BusinessLayer.Task>> returned = boardController.InProgressTask(userEmail); // TODO CHECK: All members can to this BUT JUST he is assigneed to and from all boards
            if (returned.ErrorOccured)
            {
                log.Error(returned.ErrorMessage);
                return Response<IList<Task>>.FromError(returned.ErrorMessage);
            }
            return ConvertBusinessToServiceTasksCollection(returned.Value);
        }

        /// <summary>
        /// Converts a collection of BusinessLayer Tasks into collection of ServiceLayer Tasks
        /// </summary>
        /// <param name="lst">IList of BL Tasks</param>
        /// <returns>A response object with a value set to the list of SL tasks</returns>
        private Response<IList<Task>> ConvertBusinessToServiceTasksCollection(IList<BusinessLayer.Task> lst)
        {
            IList<Task> ret = new List<Task>();
            foreach(BusinessLayer.Task t in lst)
            {
                Task toAdd = new Task(t.ID, t.CreationTime, t.Title, t.Description, t.DueDate, t.Assignee);
                ret.Add(toAdd);
            }
            return Response<IList<Task>>.FromValue(ret);
        }



        //--------------------------------------------------------
        // Whats new ! |
        //             v
        



        

        /// <summary>
        /// Adds a board created by another user to the logged-in user. 
        /// </summary>
        /// <param name="userEmail">Email of the current user. Must be logged in</param>
        /// <param name="creatorEmail">Email of the board creator</param>
        /// <param name="boardName">The name of the new board</param>
        /// <returns>A response object. The response should contain a error message in case of an error</returns>
        public Response JoinBoard(string userEmail, string creatorEmail, string boardName)
        {
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return r;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assigns a task to a user
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
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return r;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the list of board of a user. The user must be logged-in. The function returns all the board names the user created or joined.
        /// </summary>
        /// <param name="userEmail">The email of the user. Must be logged-in.</param>
        /// <returns>A response object with a value set to the board, instead the response should contain a error message in case of an error</returns>
        public Response<IList<String>> GetBoardNames(string userEmail)
        {
            Response r = IsLoggedIn(userEmail);
            if (r.ErrorOccured)
                return Response<IList<String>>.FromError(r.ErrorMessage);
            return boardController.GetBoardNames(userEmail);
        }

    }
}
