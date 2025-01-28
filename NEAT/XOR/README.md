Run code by navigating to the NEAT dir and running


for Mac users:
```
dotnet clean && rm -rf */obj/ */bin/ obj/ bin/ && dotnet build && cd Visualization && dotnet run
```

for Windows users (works in both PowerShell and CMD):
```
dotnet clean && rmdir /s /q */obj/ */bin/ obj/ bin/ && dotnet build && cd Visualization && dotnet run
```
