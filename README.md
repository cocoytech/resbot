# resbot

resbot is a CLI tool that aids in bot development by performing a few tasks:

 - Generates a DialogResponses.cs file for use with the __template manager__ pattern used in the [Bot Framework Enterprise Template](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-enterprise-template-overview?view=azure-bot-service-4.0) (optional)
 - Generates the `DialogStrings.Designer.cs` resource backing file when Visual Studio 2017 is not an option (VS Code, Mac, etc) (optional)

 ### Options

```
Usage: resbot [options]

Options:
  -v|--version                        Gets the version of resbot
  -dp|--dialogsPath </Dialogs>        The path to the folder containing all your dialogs.
  -n|--namespace <MyCompany.Bot>      The root namespace for the bot.
  -dn|--dialogName <MakeReservation>  The name of the dialog.
  -gr|--generateResponses             Generate a Responses.cs file for use with bot template manager.
  -gd|--generateDesigner              Generate a Strings.Designer.cs file for use with bot template manager.
  -?|-h|--help                        Show help information
```

### Bot Project Structure

```
.
+-- Bot.cs
+-- Dialogs
|   +-- MakeReservationDialog.cs
|   +-- MakeReservationResponses.cs
|   +-- Resources
|   |   +-- MakeReservationStrings.resx
|   |   +-- MakeReservationStrings.Designer.resx
```


### Usage

```
resbot --dialogsPath /Dialogs --namespace MyCompany.Bot --dialogName MakeReservation
```

### Output

This [`/Resources/MakeReservationStrings.resx resource file`](https://gist.github.com/rob-derosa/2ec378c3b312ee89b2e7c953d1d5e4c9) will generate these files:
  - [`/Resources/MakeReservationStrings.Designer.cs`](https://gist.github.com/rob-derosa/f9037cd372578441aaadd45538d957b6)
  - [`MakeReservationResponses.cs`](https://gist.github.com/rob-derosa/e8b9360bc79c90f67b6ba1f0dc32af61)