package main

import (
	"fmt"
	"log"
	"net"
	"os"
	"strconv"
	"time"
)

const protocol string = "tcp"

var (
	port         string = "8080"
	hostIp       string = "0.0.0.0"
	connectionId uint   = 0
)

func loadArgs() {
	argsLen := len(os.Args)
	if argsLen > 3 {
		log.Panicf("Usage %s: <ip> <port>", os.Args[0])
	}
	if argsLen > 1 {
		hostIp = os.Args[1]
	}
	if argsLen > 2 {
		port = os.Args[2]
	}
}

func getStringData(c net.Conn) (string, error) {
	buffer := make([]byte, 1024)
	len, err := c.Read(buffer)
	if err != nil {
		return "", err
	}
	return string(buffer[:(len - 1)]), nil
}

func handleConnection(conn net.Conn, id uint) {
	defer conn.Close()
	defer log.Printf("[%d] CLOSE", id)
	defer conn.Write([]byte("CLOSE"))
	clientData := fmt.Sprintf("%s://%s", conn.RemoteAddr().Network(), conn.RemoteAddr().String())
	log.Printf("[%d]New connection received %s", connectionId, clientData)
	conn.Write([]byte(fmt.Sprintf("Connecting from: %s\t", clientData)))
	var (
		data string
		err  error
		iter int
	)
	data, err = getStringData(conn)
	if err != nil {
		log.Printf("[%d]ERR: %s", id, err.Error())
		return
	}
	iter, err = strconv.Atoi(data)
	if err != nil {
		log.Printf("[%d]ERR: %s", id, err.Error())
		return
	}
	log.Printf("[%d]Number of messages: %d", id, iter)
	conn.Write([]byte(fmt.Sprintf("Waiting for: %d messages\t", iter)))
	for i := 0; i < iter; i++ {
		data, err = getStringData(conn)
		if err != nil {
			log.Printf("[%d]ERR: %s", id, err.Error())
			return
		}
		log.Printf("[%d]>>> %s", id, data)
		response := fmt.Sprintf("%s\t", time.Now().String())
		conn.Write([]byte(response))
	}
}

func main() {
	loadArgs()
	l, err := net.Listen(protocol, fmt.Sprintf("%s:%s", hostIp, port))
	if err != nil {
		log.Fatal(err)
	}
	defer l.Close()

	for {
		conn, err := l.Accept()
		if err != nil {
			log.Fatal(err)
		}
		go handleConnection(conn, connectionId)
		connectionId++
	}
}
