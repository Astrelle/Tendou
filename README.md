# Tendou

Tendou is a cross-platform visual novel development toolset. Production utilizing Tendou is split into two applications, **Kei**, which is a desktop application that functions as a creation studio, and **Arisu**, which functions as a json parsing runtime playback engine.

As the focus of this project is cross-platform implementation, a hard architectural boundary was set between the creation tool and the playback engine, allowing ports of the playback engine to easily be developed and work with previously created titles.

```mermaid
flowchart TD
    Kei["Kei (Creator Studio)"]
    JSON["story_data.json"]
    Arisu["Arisu (Playback Engine)"]
    Ports["Secondary Ports (PSP, etc.)"]

    Kei -->|Compiles & Exports| JSON
    JSON -->|Parses & Interprets| Arisu
    JSON -.->|Universal Schema| Ports

    style Kei fill:#2d1a38,stroke:#a154d3,stroke-width:2px,color:#fff
    style JSON fill:#1e1e24,stroke:#555,stroke-width:1px,color:#ddd
    style Arisu fill:#1b365d,stroke:#4a90e2,stroke-width:2px,color:#fff
    style Ports fill:#222,stroke:#444,stroke-width:1px,stroke-dasharray: 5 5,color:#aaa
```

---

## Kei - Visual Novel Creation Studio

Kei is the development environment built with C# and Avalonia UI. She serves as the organizer and is where you'll write script dialogue, assign files and assets to scenes, handle audio, and manage character layouts in scenes. With Avalonia, she handles creation across standard operating systems `Windows, Linux, MacOS` beautifully. 


