#!/bin/zsh

## Check the Register Post Method
echo "Checking the Register Post Method"
curl -i -X POST http://localhost:10001/register --header "Content-Type: application/json" -d "{\"Username\":\"Muayad\", \"Password\":\"Muayad1234\"}"
echo "Should return User registered successfully."

## Check the Login Post Method
echo "Checking the Login Post Method"
curl -i -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"Muayad\", \"Password\":\"Muayad1234\"}"
echo "Should return the generated Token for the user"

## Check the Package Get Method
echo "Checking the Package Get Method"
curl -i -X GET http://localhost:10001/package --header "Content-Type: application/json" -d "{\"Username\":\"Muayad\"}"
echo "Should return Package purchased successfully."