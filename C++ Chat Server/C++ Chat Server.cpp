#define _CRT_SECURE_NO_WARNINGS
#include <iostream>
#include <string>
#include <cstring>
#include <cstdlib>
#include <cstdio>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <thread>
#include <vector>
#include <fstream>
#include <filesystem> // Include the filesystem header

std::vector<std::string> previousMessages;

void appendMessageToFile(const std::string& message, const std::string& filename) {
    std::ofstream file(filename, std::ios::app);
    if (file.is_open()) {
        file << message << "\n";
        file.close();
    }
    else {
        std::cerr << "Error opening file for writing." << std::endl;
    }
}

void writeMessagesToFile(const std::vector<std::string>& messages, const std::string& filename) {
    std::ofstream file(filename);

    if (file.is_open()) {
        for (const std::string& message : messages) {
            file << message << std::endl;
        }
        file.close();
    }
    else {
        std::cerr << "Error opening file for writing." << std::endl;
    }
}

void handleClient(SOCKET clientSocket, std::vector<SOCKET>& clients) {
    char buffer[1024];
    std::ifstream file("chat_history.txt");
    std::string line;

    // Check if the file exists; if not, create it
    if (std::filesystem::exists("chat_history.txt")) {
        std::ifstream file("chat_history.txt");
        if (file.is_open()) {
            std::string line;
            while (std::getline(file, line)) {
                send(clientSocket, line.c_str(), line.length(), 0);
            }
            file.close();
        }
        else {
            std::cerr << "Error opening chat history file for reading." << std::endl;
        }
    }


    while (1) {
        int bytesReceived = recv(clientSocket, buffer, sizeof(buffer), 0);
        if (bytesReceived <= 0) {
            std::cerr << "Client disconnected." << std::endl;
            closesocket(clientSocket);

            // Remove the client socket from the list
            for (auto it = clients.begin(); it != clients.end(); ++it) {
                if (*it == clientSocket) {
                    clients.erase(it);
                    break;
                }
            }
            return;
        }

        buffer[bytesReceived] = '\0';

        std::cout << buffer;

        // Broadcast the received message to all clients
        for (const SOCKET& client : clients) {
            if (client != clientSocket) {
                send(client, buffer, bytesReceived, 0);
            }

        }

        // Store the message in the previousMessages vector
        previousMessages.push_back(buffer);
        writeMessagesToFile(previousMessages, "chat_history.txt");
    }
}


int main() {
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        std::cerr << "WSAStartup failed." << std::endl;
        return 1;
    }

    SOCKET serverSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (serverSocket == INVALID_SOCKET) {
        std::cerr << "Error in socket" << std::endl;
        WSACleanup();
        return 1;
    }

    sockaddr_in serverAddr;
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(8080); // Use the same port as in the client
    serverAddr.sin_addr.s_addr = INADDR_ANY; // Bind to any available network interface

    if (bind(serverSocket, (struct sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Error in binding. Port may already be in use or permissions issue." << std::endl;
        closesocket(serverSocket);
        WSACleanup();
        return 1;
    }

    if (listen(serverSocket, SOMAXCONN) == SOCKET_ERROR) {
        std::cerr << "Error in listening" << std::endl;
        closesocket(serverSocket);
        WSACleanup();
        return 1;
    }

    std::cout << "Server is listening for incoming connections." << std::endl;

    std::vector<SOCKET> clients;

    while (1) {
        SOCKET clientSocket = accept(serverSocket, NULL, NULL);
        if (clientSocket == INVALID_SOCKET) {
            std::cerr << "Error in accepting client connection" << std::endl;
            closesocket(serverSocket);
            WSACleanup();
            return 1;
        }
        else {
            std::cout << "Client connected." << std::endl;
        }

        clients.push_back(clientSocket);

        std::thread clientThread(handleClient, clientSocket, std::ref(clients));
        clientThread.detach();
    }

    closesocket(serverSocket);
    WSACleanup();
    return 0;
}
