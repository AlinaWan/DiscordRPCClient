<div align="center">
  <h1>Discord RPC Client</h1>
</div>

<div align="center">

[![License](https://img.shields.io/github/license/AlinaWan/DiscordRPCClient)](LICENSE)
[![C#](https://custom-icon-badges.demolab.com/badge/C%23-%23239120.svg?logo=cshrp&logoColor=white)](#)
[![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff)](#)
[![Windows](https://custom-icon-badges.demolab.com/badge/For%20Windows-0078D6?logo=windows11&logoColor=white)](#)
[![❤︎](https://img.shields.io/badge/Made%20with%20%E2%9D%A4%20by%20Riri-FFCAE9)](#)


A fully free, open-source, and unopinionated Discord Custom Rich Presence editor.

  <img src="assets/preview.webp" alt="Preview" width="100%">
</div>

---

## 🛠️ Yet Another RPC Client‽

Here is what makes this client different:

* Open Source (No Black Boxes)
* No Forced Advertisements
* Complete Field Support
* Exportable/Importable Config
* A Real GUI (No Plaintext Editing)
* UDP Payload Support for Values
* The App is Just a Couple Hundred Kilobytes

---

## ⚙️ Technical Specification

The client is built on the [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) framework (C#) with WPF and is powered by [DiscordRichPresence](https://www.nuget.org/packages/DiscordRichPresence) by [Lachee](https://www.nuget.org/profiles/Lachee).

<details>
  <summary>UDP payload schema</summary>

```
{
  "ClientId": "1234567890123456",
  "Details": "Exploring the Dungeon",
  "State": "In a Party",
  "UseTimestamps": true,
  "AsTimeRemaining": true,
  "TotalDurationMinutes": 45,
  "LargeImageKey": "logo_large",
  "LargeImageText": "Main Expansion Logo",
  "SmallImageKey": "status_online",
  "SmallImageText": "Level 50 Warrior",
  "PartyId": "party_12345_abcde",
  "PartyCurrentSize": 2,
  "PartyMaxSize": 4,
  "JoinSecret": "join_secret_token_xyz",
  "SpectateSecret": "spectate_secret_token_123",
  "Button1Label": "Visit Website",
  "Button1Url": "https://example.com",
  "Button2Label": "Join Discord",
  "Button2Url": "https://discord.gg/invite"
}
```

</details>


<details>
  <summary>For nerds</summary>

  Persistent state path: <code>%USERPROFILE%\AppData\Local\DiscordRPCManager</code>  
  Assembly name: <code>DiscordRPCManager</code>  
  Target framework: <code>net10.0-windows10.0.19041.0</code>

</details>

---

## 📄 License

This project is entirely free and open-source under the very permissive [MIT License](LICENSE).

---

*Made with ♡ by Riri*
