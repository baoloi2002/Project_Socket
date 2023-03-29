import json

# Open the input text file
with open('input.txt', 'r') as f:
    lines = f.readlines()

# Split the lines into groups of 6 (question + 4 choices + answer)
groups = [lines[n:n+6] for n in range(0, len(lines), 6)]

# Convert each group into a dictionary object
questions = []
for group in groups:
    question = {
        "question": group[0].strip(),
        "choices": [choice.strip() for choice in group[1:5]],
        "answer": int(group[5].strip())
    }
    questions.append(question)

# Write the questions to a JSON file
with open('output.json', 'w') as f:
    json.dump(questions, f, indent=4)
