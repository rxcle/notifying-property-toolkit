@echo off
dotnet dotcover test --dcReportType=html
start dotCover.Output.html