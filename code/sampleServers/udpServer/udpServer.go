package main

import (
	"fmt"
	"log"
	"net"
	"os"
	"time"
)

const protocol string = "udp"

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
func response(udpServer net.PacketConn, addr net.Addr, buffer []byte) {
	var (
		clientData = string(buffer)
		clientAddr = fmt.Sprintf("%s://%s", addr.Network(), addr.String())
		response   = fmt.Sprintf(
			"time: %s | client: %s | data: %s | \t",
			time.Now().String(), clientAddr, clientData)
	)
	log.Printf("[%d]New connection received %s", connectionId, clientAddr)
	log.Printf("[%d]>>> %s", connectionId, clientData)

	udpServer.WriteTo([]byte(response), addr)
}
func main() {
	loadArgs()
	udpServer, err := net.ListenPacket(protocol, fmt.Sprintf("%s:%s", hostIp, port))
	if err != nil {
		log.Fatal(err)
	}
	defer udpServer.Close()

	for {
		buffer := make([]byte, 1024)
		len, addr, err := udpServer.ReadFrom(buffer)
		if err != nil {
			log.Fatal(err)
		}

		go response(udpServer, addr, buffer[:(len-1)])
		connectionId++
	}
}
