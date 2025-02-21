import matplotlib.pyplot as plt
import pandas as pd
import os

def plot_results(csv_path):
    data = pd.read_csv(csv_path)
    plt.figure(figsize=(10, 6))
    plt.plot(data['Input'], data['Expected'], label='Expected', color='blue')
    plt.plot(data['Input'], data['Actual'], label='Actual', color='red', linestyle='dashed')
    plt.xlabel('Input')
    plt.ylabel('Output')
    plt.title('Sine Function Approximation')
    plt.legend()
    plt.grid(True)
    plt_path = os.path.splitext(csv_path)[0] + '.png'
    plt.savefig(plt_path)
    plt.show()
    print(f"Plot saved to: {plt_path}")

if __name__ == "__main__":
    csv_path = os.path.join(os.path.dirname(__file__), 'plot_data.csv')
    plot_results(csv_path)
