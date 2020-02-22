using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Obsolete("CommandTypes should not be used anymore!", true)]
// The commands for interaction between the server and the client
enum Command {
    Ping,    // Just to find the server
    Login,   // Log into the server
    Logout,  // Logout of the server 
    Message, // Send a text message to all the chat clients     
    List     // Get a list of users in the chat room from the server
}