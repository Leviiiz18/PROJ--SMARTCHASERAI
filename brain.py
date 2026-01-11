import neat
import socket
import os
import math
import time
import pickle

HOST = "127.0.0.1"
PORT = 5005

# ===== TRAINING SETTINGS =====
GENOME_TIME = 0.6
MAX_GENERATIONS = 50

CAPTURE_DISTANCE = 5.0
CAPTURE_REWARD = 300.0      # 🔥 MAIN GOAL
MAX_CAPTURES = 1

SURVIVAL_REWARD = 0.03      # 🧠 exploration
PROGRESS_SCALE = 1.2        # 🧭 guidance

# ===== LOAD NEAT CONFIG =====
config_path = os.path.join(os.path.dirname(__file__), "config-feedforward.txt")
config = neat.Config(
    neat.DefaultGenome,
    neat.DefaultReproduction,
    neat.DefaultSpeciesSet,
    neat.DefaultStagnation,
    config_path
)

config.pop_size = 30
config.fitness_threshold = 300.0   # reachable

population = neat.Population(config)
population.add_reporter(neat.StdOutReporter(True))
population.add_reporter(neat.StatisticsReporter())

# ===== SOCKET SETUP =====
server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((HOST, PORT))
server.listen(1)

print("Waiting for Unity...")
conn, addr = server.accept()
print("Connected:", addr)

conn.settimeout(1.0)

current_generation = 0
recv_buffer = ""

# ===== TRAINING LOOP =====
def eval_genomes(genomes, config):
    global current_generation, recv_buffer
    current_generation += 1
    genome_index = 0

    for genome_id, genome in genomes:
        genome_index += 1

        net = neat.nn.FeedForwardNetwork.create(genome, config)
        genome.fitness = 0.0

        prev_distance = None
        start_time = time.time()
        captured = False

        while time.time() - start_time < GENOME_TIME and not captured:
            try:
                chunk = conn.recv(1024).decode()
            except socket.timeout:
                continue

            if not chunk:
                return

            recv_buffer += chunk

            while "\n" in recv_buffer:
                line, recv_buffer = recv_buffer.split("\n", 1)
                line = line.strip()
                if not line:
                    continue

                # ===== INPUT =====
                try:
                    dx, dz = map(float, line.split(","))
                except ValueError:
                    continue

                distance = math.sqrt(dx * dx + dz * dz)

                # ===== RUN NETWORK =====
                output = net.activate([dx, dz])
                move_x = max(-1, min(1, output[0] * 2))
                move_z = max(-1, min(1, output[1] * 2))

                # ===== FITNESS =====
                genome.fitness += SURVIVAL_REWARD

                if prev_distance is not None:
                    delta = prev_distance - distance
                    if delta > 0:
                        genome.fitness += delta * PROGRESS_SCALE

                prev_distance = distance

                # ===== CAPTURE =====
                if distance <= CAPTURE_DISTANCE:
                    genome.fitness += CAPTURE_REWARD
                    conn.sendall(b"RESET\n")
                    captured = True
                    break

                # ===== SEND TO UNITY =====
                message = (
                    f"{move_x},{move_z}|"
                    f"{dx},{dz}|"
                    f"{current_generation},{genome_index},"
                    f"{genome.fitness:.2f},{distance:.2f}\n"
                )
                conn.sendall(message.encode())

        # 🔒 Mild floor (no collapse)
        if genome.fitness < -5:
            genome.fitness = -5


# ===== TRAIN AND SAVE =====
winner = population.run(eval_genomes, MAX_GENERATIONS)

with open("HS3.pkl", "wb") as f:
    pickle.dump(winner, f)

print("✅ Training finished")
print("✅ Best genome saved as HS3.pkl")
