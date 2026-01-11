import neat
import socket
import os
import pickle

HOST = "127.0.0.1"
PORT = 5005

# ===== LOAD NEAT CONFIG =====
config_path = os.path.join(os.path.dirname(__file__), "config-feedforward.txt")
config = neat.Config(
    neat.DefaultGenome,
    neat.DefaultReproduction,
    neat.DefaultSpeciesSet,
    neat.DefaultStagnation,
    config_path
)

# ===== LOAD TRAINED GENOME =====
with open("best_genome.pkl", "rb") as f:
    genome = pickle.load(f)

net = neat.nn.FeedForwardNetwork.create(genome, config)

# ===== SOCKET SETUP =====
server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((HOST, PORT))
server.listen(1)

print("Waiting for Unity (DEMO MODE)...")
conn, addr = server.accept()
print("Connected:", addr)

conn.settimeout(1.0)
recv_buffer = ""

while True:
    try:
        chunk = conn.recv(1024).decode()
        if not chunk:
            break

        recv_buffer += chunk

        # 🔥 PROCESS COMPLETE INPUT LINES
        while "\n" in recv_buffer:
            line, recv_buffer = recv_buffer.split("\n", 1)
            line = line.strip()
            if not line:
                continue

            # Parse inputs
            dx, dz = map(float, line.split(","))

            # Run neural network
            output = net.activate([dx, dz])

            # Same scaling as training
            move_x = max(-1, min(1, output[0] * 2))
            move_z = max(-1, min(1, output[1] * 2))

            # 🔥 SEND RESPONSE WITH NEWLINE
            message = (
                f"{move_x},{move_z}|"
                f"{dx},{dz}|DEMO\n"
            )
            conn.sendall(message.encode())

    except socket.timeout:
        continue
    except Exception as e:
        print("Connection error:", e)
        break

conn.close()
print("Disconnected.")
