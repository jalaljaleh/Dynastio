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


# 🏰 Dynast.io Bot

> A powerful, open-source Discord bot tailored for [Dynast.io](https://dynast.io) — the fast-paced multiplayer survival game.  
> Built with ❤️ using **.NET Core**, **C#**, and **MongoDB**.

![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0+-purple)
![MongoDB](https://img.shields.io/badge/MongoDB-Atlas-green)
![Discord](https://img.shields.io/badge/Discord-Bot-5865F2)

---

## 🎮 What is Dynast.io?

**Dynast.io** is a browser-based, 2D multiplayer survival game where players battle the elements — and each other — to build, survive, and thrive.  
With up to 100 players per session, the game combines crafting, combat, exploration, and RPG mechanics in a dynamic world that evolves with every match.

Whether you're building a fortress, hunting wild beasts, or teaming up with friends, Dynast.io offers a rich survival experience with real-time action and strategy.

---

## 🤖 Introducing Dynast.io Bot

**Dynastio Bot** is a community-driven Discord bot designed to bridge the gap between **Dynast.io gameplay** and **Discord interaction**.  
It’s independently developed but officially approved and actively used on the **Dynast.io Discord server**.

The bot provides real-time game data, moderation tools, and multilingual support — all wrapped in a modular, scalable architecture that’s easy to deploy and extend.

---

## 🚀 Key Features

- ✅ **Dynast.io API Integration**  
  Access live player stats, leaderboards, and game events directly from Discord.

- 🔒 **Moderation Tools**  
  Manage your server with built-in commands for moderation and automation.

- 🌍 **Multi-language Support**  
  Supports English, Thai, and Ukrainian with a fallback system for localization.

- 📊 **MongoDB Persistence**  
  Stores user data, settings, and game sync info using MongoDB Atlas or local instances.

- 📂 **Modular Architecture**  
  Clean separation of concerns for scalability and maintainability.

- 🖼 **Image-Only Channel Enforcement**  
  Enforce media-only channels with smart message filtering.

- 📘 **Localized Messaging**  
  Dynamic message translation with fallback logic for unsupported languages.

- 🧪 **Unit Testing Framework**  
  Structured test project for validating bot logic and API interactions.

- 🧠 **Lightweight Logic Layers**  
  Includes validation, global state management, and API wrappers via Dynastio.Net.

- 🧰 **Cloud-Ready Deployment**  
  Compatible with Docker, Heroku, and other cloud platforms.




## 🗺 Dynast.io Bot Roadmap

This roadmap outlines planned features, improvements, and long‑term goals for the project.  
Items are grouped by priority and stage of development.

---

### ✅ Completed
- Full integration with **Dynast.io APIs** for live stats and leaderboards
- Discord **moderation tools** (kick, ban, mute, etc.)
- Multi‑language support (**English**, **Russian**) with fallback system
- Persistent storage using **MongoDB**
- Modular, scalable architecture
- Unit tests and structured testing project
- Docker, Heroku, and cloud deployment compatibility

---

### 🚧 In Progress
- Slash command migration for modern Discord API support
- Real‑time in‑game event notifications
- Enhanced player profile linking with verification
- Extended API wrappers in **Dynastio.Net**
- Additional admin moderation tools (temp bans, automated warnings)

---

### 📅 Planned
- Web dashboard for bot configuration and analytics
- Seasonal leaderboard auto‑reset and rewards
- Rich embed templates for game events
- More localized language packs (Spanish, Russian, French)
- Automated role assignment based on in‑game achievements
- Integration with Dynast.io friend and party systems

---

### 💡 Ideas Under Consideration
- **Mini‑games** within Discord themed around Dynast.io
- Player economy tracking and trade statistics
- Achievement showcase commands
- Custom badge creation tool
- Twitch/YouTube streaming notifications for Dynast.io content creators

---

**📌 Note:**  
This roadmap is flexible — priorities may shift based on community feedback, game updates, and contributor availability.  
If you have suggestions, please [open an issue](https://github.com/jalaljaleh/Dynastio.Bot/issues) or start a discussion.








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

