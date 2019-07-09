import pandas as pd
import csv
import matplotlib.pyplot as plt
df = pd.read_csv('dungeon.csv')

x = df['generation']
y = df['non-linearity']

plt.plot(x,y)
plt.ylabel('Dungeon Non Linearity')
plt.xlabel('Generation')
plt.title('Dungeon Designer Critical Linearity')
plt.show()