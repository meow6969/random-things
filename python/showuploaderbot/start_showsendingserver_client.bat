@echo off

call ensure_venv.bat
call venv\Scripts\activate.bat

cd showsendingserver
python client.py

set /P meow="press enter to exit..."
