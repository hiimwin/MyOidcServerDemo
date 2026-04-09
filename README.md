**📌 MY OIDC SERVER DEMO**

A simple learning project to understand how OpenID Connect (OIDC) works internally, combined with basic CI/CD automation using Jenkins and Docker.

**⚠️ IMPORTANT**

This is a **LEARNING PROJECT ONLY.**

❌ Not production-ready
❌ Not using Clean Architecture
❌ Not fully following SOLID

👉 The purpose is to:

Understand OIDC flow step-by-step
Learn how authentication works internally
Practice basic CI/CD automation
Get familiar with Docker workflow

**🎯 OBJECTIVES**

This project helps you:

🔐 Understand OIDC Authentication Flow
🔄 Simulate login & authentication manually
🍪 Learn cookie-based session handling
⚙️ Practice:
Jenkins Pipeline
Docker build process
Basic CI/CD flow

**🏗️ PROJECT STRUCTURE**
MyOdicServer/
 ├── OidcServer/        # OIDC Provider (Auth Server)
 ├── OidcWebClient/     # Web Client

 ├── MyOdicServer.slnx

 ├── Dockerfile.server
 ├── Dockerfile.client

 ├── docker-compose.yml
 ├── docker-compose.dev.yml

 ├── Jenkinsfile        # CI/CD pipeline
 ├── README.md
 
**🔄 APPLICATION BEHAVIOR**
🧑‍💻 LOGIN FLOW
User opens Web Client
User enters a username
System checks:
✅ Correct (hardcoded user) → Continue OIDC flow
❌ Incorrect → Return "User not found"

**🔐 AUTHENTICATION PROCESS**

If user is valid:

Redirect to OIDC Server
Server handles authentication
Return result to client
Client receives authentication response

**👤 AFTER LOGIN**

Redirect to Profile page
Session is stored using cookies

**🍪 COOKIE BEHAVIOR**

✅ Not logout → Session persists
🔓 Logout → Cookies cleared → Session removed
🚀 RUN LOCALLY
1️⃣ Clone project
git clone https://github.com/hiimwin/MyOidcServerDemo.git
cd MyOidcServerDemo
2️⃣ Run OIDC Server
cd MyOdicServer/OidcServer
dotnet run
3️⃣ Run Client
cd MyOdicServer/OidcWebClient
dotnet run
4️⃣ Test Flow
Open Web Client
Enter username:
✅ Correct → Login success
❌ Wrong → "User not found"
After login:
Access Profile
Logout to clear cookies

**🐳 DOCKER SETUP**

Run with Docker:

docker-compose -f docker-compose.dev.yml up --build

**⚠️ DOCKER BEHAVIOR (IMPORTANT)**

✅ Docker will:
Build images
Run containers for testing
❌ Docker will NOT automatically remove images
🧹 Containers:
Can be stopped/removed after testing

👉 Images are kept intentionally so you can:

Run containers manually later
Debug or test independently
⚙️ CI/CD WITH JENKINS

This project includes a basic Jenkins pipeline for automation.

**🔁 PIPELINE FLOW**

Pull source code from GitHub
Build .NET project
Build Docker images:
Server image
Client image
Run test containers
Stop/remove containers after testing
📄 JENKINSFILE PURPOSE
Automate build process
Integrate Docker workflow
Simulate basic CI/CD pipeline

👉 This is basic level CI/CD for learning, not production setup.

**❗ LIMITATIONS**

This project intentionally does NOT include:

Clean Architecture
Full SOLID design
Database (uses hardcoded user)
Advanced OIDC features:
PKCE
Refresh Token
Role/Claim system
Production security

**💡 WHY THIS PROJECT EXISTS**

Instead of using ready-made libraries, this project:

👉 Rebuilds OIDC flow manually

So you can:

Understand authentication deeply
See what happens behind the scenes
Practice DevOps basics (CI/CD + Docker)

**🚀 FUTURE IMPROVEMENTS (OPTIONAL)**
Replace hardcoded user with database
Add JWT token handling
Implement PKCE
Apply Clean Architecture
Deploy to cloud (AWS / Azure)

**⭐ FINAL NOTE**

This project is best used as:

🧪 OIDC learning playground
⚙️ CI/CD practice project
🐳 Docker hands-on demo
