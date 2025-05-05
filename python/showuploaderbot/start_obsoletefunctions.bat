@echo off

call ensure_venv.bat
call venv\Scripts\activate.bat

python obsoletefunctions.py

set /P meow="press enter to exit..."
