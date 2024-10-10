#!/bin/zsh

## Check the Register Post Method for player 1
echo "Checking the Register Post Method"
curl -i -X POST http://localhost:10001/register --header "Content-Type: application/json" -d "{\"Username\":\"Muayad\", \"Password\":\"Muayad1234\"}"
echo "\nShould return User registered successfully."

## Check the Login Post Method for player 1
echo "Checking the Login Post Method"
curl -i -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"Muayad\", \"Password\":\"Muayad1234\"}"
echo "\nShould return the generated Token for the user"

## Check the Package Get Method for player 1
echo "Checking the Package Get Method"
curl -i -X GET http://localhost:10001/package --header "Content-Type: application/json" -d "{\"Token\":\"rX+Fc1AOENCqPuxMtORcLneSJn31gUlnPRyT525cYgk=\"}"
echo "\nShould return Package purchased successfully."

## Check the Register Post Method for player 2
echo "Checking the Register Post Method"
curl -i -X POST http://localhost:10001/register --header "Content-Type: application/json" -d "{\"Username\":\"Mostafa\", \"Password\":\"Muayad1234\"}"
echo "\nShould return User registered successfully."

## Check the Login Post Method for player 2
echo "Checking the Login Post Method"
curl -i -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"Mostafa\", \"Password\":\"Muayad1234\"}"
echo "\nShould return the generated Token for the user"

## Check the Package Get Method for player 2
echo "Checking the Package Get Method"
curl -i -X GET http://localhost:10001/package --header "Content-Type: application/json" -d "{\"Token\":\"Cahh1e/7w+ppu9nqhuRFep7ek1gFWRzWS6cTQtb62Z0=\"}"
echo "\nShould return Package purchased successfully."

## Check the Battle Put Method
echo "\nChecking the Battle Put Method"
##....
