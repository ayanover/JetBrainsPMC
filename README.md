# Custom Console - JetBrainsPMC

Project created as for a JetBrains internship application. The task was to create an app that has an input that takes in commands with arguments and returns a formatted output in another field.

# Development process

At first I knew only about the process library in .NET that lets me launch a Process with given arguments. This would let me run a cmd.exe or powershell with a command as an argument. So I went with this design and created first cli. 
![image](https://github.com/user-attachments/assets/c3cea0b0-eac8-4364-8679-654f61f6482f)

It works well enough - it executes one command at a time, formats the output based on the output type (stdout or stderr).
The problem is that is has a major flaw - whenever a nested cli app is launched by command like python REPL or mysql. The program just waits indifninetely as the console process waits for additional input, and I didn't find a way to send another command to that instance.
At first I thought there has to be a solution that will keep my current implementation using the Proccess library but everything and anything I tried required some hard-coding or using practises that were... well... impractical. And Even with these, it didn't even work as expected. 

Then I gave myself some time to think and do some more research, I also took a break from the project as my uni and freelancing job needed some more attention. At this time, I submitted the first solution with the plan on refining it later.

After some time I learned about ConPTY or a pseudoconsole, which lets me execute commands on a console by calling an API with the command. The communication happens through pipes. It then returns the output with VT100 formatting. Next thing was implementing proper text formatting so all the output doesnt look like "[[m9001".
But before that, I had to make the program actually work. For the first solution and start of the second one, I used Rider IDE. However in the second one, something was off, Even after several tries and hours of debugging, I couldn't get it to work. At this time I found a thread that let me know about an issue of debugger eating up contents
of a pipe. 

