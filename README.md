<meta name="google-site-verification" content="UadymurN9NAR6VJSIlqjA_MBnvcT16Qo2EJIUK7qBeU" />

&#x20;
<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="#">
    <img src="logo.jpg" alt="Logo" width="150" height="150">
  </a>

  <h3 align="center">Dynast.io Bot</h3>

  <p align="center">
    A .net core discord bot project for dynast.io game.
    <br />
    <br />
    <a href="https://discord.gg/GVUXMNv7vV">View Bot</a>
    ·
    <a href="https://github.com/jalaljaleh/Dynastio.Discord/issues">Report Bug</a>
    ·
    <a href="https://github.com/jalaljaleh/Dynastio.Discord/issues">Request Feature</a>
  </p>
</div>

<div align="center">
  
  
[![Profile](https://komarev.com/ghpvc/?username=jalaljaleh-dynastio&style=flat-square)](https://discord.gg/x5j4cZtnWR)
[![Discord](https://discord.com/api/guilds/875716592770637824/widget.png)](https://discord.gg/x5j4cZtnWR)
  
</div>

## 🧠 About the Project

**Dynastio Bot** is an open-source Discord bot created for **Dynast.io**, a popular multiplayer survival browser game. This project is independently developed but **approved and officially used** on the Dynast.io Discord server.

The goal is to deliver an immersive community tool that:

- Syncs with Dynast.io gameplay and user data
- Supports multilingual player bases
- Is lightweight, efficient, and scalable
- Enables further customization via open API wrappers (`Dynastio.Net`)

> Built with ❤️ using **.NET Core**, **C#**, and **MongoDB**

## 🚀 Features

- ✅ Full integration with Dynast.io APIs
- 🔒 Discord server moderation tools
- 🌍 Multi-language support (English, Thai, Ukrainian)
- 📊 MongoDB-based persistent storage
- 📂 Modular architecture for scalability
- 🖼 Image-only channel enforcement
- 📘 Localized messages with fallback system
- 🧪 Unit testing and structured test project
- 🧠 Lightweight logic layers (Validation, Global state, API wrappers)
- 🧰 Compatible with Docker, Heroku & cloud deployment

## 🛠 Getting Started

These instructions will get you a copy of the project up and running on your local machine.

### Prerequisites

- .NET SDK 6 or later
- A [Discord bot token](https://discordjs.guide/preparations/setting-up-a-bot-application.html#creating-your-bot)
- A [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) (or local MongoDB instance)
- Your own Dynast.io API key ([Docs](https://github.com/jalaljaleh/Dynastio.Net))

### Installation

```bash
git clone https://github.com/jalaljaleh/Dynastio.git
cd Dynastio/Dynastio.Bot
dotnet restore
dotnet run
```

Set your secrets or environment variables:

```json
{
  "DiscordToken": "YOUR_TOKEN",
  "MongoDbConnectionString": "YOUR_MONGO_URI",
  "CommandPrefix": "=",
  "OwnerId": "YOUR_DISCORD_ID"
}
```

## 🗺 Roadmap

-

> Suggestions welcome! Open an issue or PR anytime.

## 🤝 Contributing

Contributions are what make the open source community amazing. Fork the repo, add features, fix bugs — and submit pull requests!

### Contribution Steps:

1. Fork the repo
2. Create a feature branch
3. Commit your changes
4. Push the branch
5. Open a Pull Request

Or open an [Issue](https://github.com/jalaljaleh/Dynastio/issues) with the `enhancement` label.

## 📄 License

Distributed under the **Apache-2.0 License**. See `LICENSE.txt` for full details.

## 📬 Contact

- Author: [Jalal Jaleh](https://github.com/jalaljaleh)
- Discord: [Halun](https://discord.gg/x5j4cZtnWR)
- Dynastio API: [Dynastio.Net](https://github.com/jalaljaleh/Dynastio.Net)

Project Link: [https://github.com/jalaljaleh/Dynastio](https://github.com/jalaljaleh/Dynastio)

