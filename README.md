# Cloudflare DNS Updater

---

## ☝🏼 - Program Description

This program is designed to automatically update your **Cloudflare DNS (Domain Name System)** record by assigning your current public Internet Protocol (IP) address as the target destination. This process ensures that your domain name always remains correctly directed to your server or network, even in circumstances where your IP address changes due to dynamic allocation by your Internet Service Provider (ISP). By automating this task, the program eliminates the need for manual updates and significantly reduces the risk of downtime or connectivity issues for services hosted on your domain.

---

## 📔 - Version Information

> [!WARNING]  
> I no longer officially provide support for this program.


Current version: **1.2.2.3** Release date: **20/12/2023**

Latest Updates:

- More detailed error messages.
- Source Code was optimised and cleaned
    
    ---
    
    ## ⚠️ - Software Requirements
    

- Windows 8 (or higher)
- Cloudflare Account
- Domain
- API token

---

## 🚩 - Preparation

### Cloudflare Sign up

1.  Head over to [Cloudflare’s official website](https://www.cloudflare.com/)
2.  Sign up for an Account or log in to your existing Account

### Get your Cloudflare Zone-ID

1.  Select the ‘Overview’ tab
2.  Locate the ‘API’ section
3.  Copy your Zone-ID and past it into a text document

### Get your Cloudflare API Token

1.  Click on your account icon
2.  Click on ‘My Profile’
3.  Click on ‘API Tokens’
4.  Click the button labelled ‘Create Token’
5.  Select the template called ‘Edit Zone DNS’
6.  Select your Zone (Domain), listed at ‘Zone Resources’
7.  Click on ‘Continue to summary’ and on ‘Create Token’
8.  Copy your API Token and past it into the same document as your Zone-ID

### Download the Program

1.  Head over to my website
2.  Click on ‘Download (Win 64-Bit)’
3.  Save the ZIP-Archive `CloudflareDNSUpdater_release.zip` in your desired location

### Unzipping the Program

1.  Open your file explorer and head over to the ZIP-Archive
2.  Double-click the archive to open it
3.  Click on ‘Extract all’
4.  Choose a location on your Computer where the program should be saved e.g. `C:\Program Files\DNS Updater\`

---

## 💿 - Setup

1.  Open up the executable in your directory called `CloudflareDNS.exe`
2.  Enter the setup mode by typing `setup` into the command prompt and hitting the enter key
3.  You’ll be asked some questions, answer them as follows
    1.  Enter your API-Token
    2.  Enter your Zone-ID
    3.  Enter your Record-ID from the printed DNS-Record list

---

## ⏯️ - Test Application

1.  Type `update` into the command prompt and hit your enter key
2.  The program should print a success message
    1.  **If so…**
        1.  head over to your Cloudflare Dashboard
        2.  Check your selected DNS-Record (After a successful cycle, your DNS-Record will have a comment attached (`Updated by PIN0L33KZ on: <TimestampOfUpdate>`)
    2.  **If not…**
        1.  Check your DNS-Record type in Cloudflare (A = IPv4 and AAAA = IPv6)
        2.  Reset the Application and redo the Setup

---

## 🧩 - Program Arguments

| Argument | Effect |
| --- | --- |
| `autoupdate` | Automatically sends your client’s public IP-Address to Cloudflare and exits the Program (Program must be setup!) |

> [!IMPORTANT]  
> If the application does not exit automatically, it might have detected an error during the update cycle. Check the Console for more Information

---

## ⏭️ - Automate the Update Cycle

1.  Download my \*.XML Windows Task Scheduler template
    
    [Task_Template.xml](/api/files/0198b840-302a-76b9-89a3-b4b8f3af18b1/Task_Template.xml)
    
2.  Open the Windows Task Scheduler
3.  Click on ‘Import Task’
4.  Select the downloaded \*.XML template file
5.  In the ‘General’ tab, click on the button ‘Change User or Group’
    1.  Enter your Windows Account name and click on the button ‘Check Names’ confirm the dialogue
6.  In the ‘Actions’ tab, select the only available action with type ‘start program’
    1.  Click on the ‘Edit’ button
    2.  Click on ‘Browse’ and select the Executable on your PC `CloudflareDNS.exe`
    3.  Confirm the dialogues

> [!NOTE]  
> The program will now run every day at 00:00 o’clock **(PC must be running)**
