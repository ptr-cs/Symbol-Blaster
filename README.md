# Symbol-Blaster
Game with modern user interface (UI) inspired by retro arcade cabinets. Written in C# using Windows Presentation Foundation (WPF). Prototyped in Adobe XD.

![Symbol Blaster - prototype](/media/symbol_blaster_prototype.gif "Symbol Blaster - prototype")

# Summary
&ensp;&ensp;&ensp;&ensp;Inspired by retro arcade game mechanics and visuals, Symbol Blaster is an old-school, 2-D shooter game with modern twists. Whereas many games from the "Golden Age" of arcade systems featured static, pre-defined game object geometries, Symbol Blaster allows the player to customize the geometries and colors of all major game objects ("Sprites") in real-time. Changing the geometry of a Sprite in Symbol Blaster also changes that Sprite's underlying hit-test geometry, resulting in subtle game-play variations based on the sizes and shapes of customized Sprites.

![Symbol Blaster - overview](/media/symbol_blaster_overview.gif "Symbol Blaster - overview")

# How It Works
&ensp;&ensp;&ensp;&ensp;Symbol Blaster features a Model-View-ViewModel (M-V-VM) architecture to implement the UI and game world. A MainViewModel governs the state and behavior of the UI, while a GameViewModel governs the state and behavior of the game. The eXtensible Application Markup Language (XAML) code that defines the "skeleton" of the UI incorporates the ViewModel logic through various data-bindings that enable the user to influence application state. Since the UI navigation topology is fixed at compile time, navigation is handled by a simple TabControl populated with TabItems that contain collapsible sections. Both the front-end XAML and the back-end ViewModel components are modular in order to foster development efficiency and easier maintenance. NUnit is the unit testing framework used to conduct automated testing of the application.

For a more detailed explanation of the code, [a blog post about the project is available here](http://www.dividebyzeno.com/symbol-blaster-game-part1.html).

# Features
- Modern "flat" UI prototyped in Adobe XD and implemented in XAML
- User Experience (UX) features for streamlined usage:
  - informative tooltips
  - dynamically resizable text
  - game control mapping visualization
  - form submission management
  - fluid-resize of applicaton
- Rudimentary game mechanics
- Custom update loop rendering performed via a CompositionTarget.Rendering event handler
- Unit tested with NUnit

# Setup
&nbsp;&nbsp;&nbsp;&nbsp;The repository consists of two Visual Studio projects - the SymbolBlaster project and an NUnit test project. Opening the SymbolBlaster/SymbolBlaster.sln file in Visual Studio should allow both projects to be built and run.

# Screenshots
## Adobe XD Artboards
![Symbol Blaster - artboards](/media/symbol_blaster_artboards.gif "Symbol Blaster - artboards")
## Comparison of Adobe XD prototype UI (left) with XAML implementation (right)
![Symbol Blaster - comparison](/media/symbol_blaster_comparison.gif "Symbol Blaster - comparison")
## Game Object Sprite Selector
![Symbol Blaster - sprites](/media/symbol_blaster_sprites.gif "Symbol Blaster - sprites")
## Color Selector UserControl
![Symbol Blaster - color selector](/media/symbol_blaster_color_selector.gif "Symbol Blaster - color selector")
## Game Objects
![Symbol Blaster - game objects](/media/symbol_blaster_game_objects.gif "Symbol Blaster - game objects")
## Configuration Presets
![Symbol Blaster - presets](/media/symbol_blaster_presets.gif "Symbol Blaster - presets")
![Symbol Blaster - custom](/media/symbol_blaster_custom.gif "Symbol Blaster - custom")
