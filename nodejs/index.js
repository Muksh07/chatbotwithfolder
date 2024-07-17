const express = require('express');
const { GoogleGenerativeAI } = require('@google/generative-ai');
require('dotenv').config();

const app = express();
const port = 5000;

// Middleware to parse JSON requests
app.use(express.json());

// Ensure the API key is correctly loaded from the environment variable
const apiKey = process.env.API_KEY;


const genAI = new GoogleGenerativeAI(apiKey);
const model = genAI.getGenerativeModel({ model: 'gemini-1.5-pro' });

app.post('/generate', async (req, res) => {
  const { prompt } = req.body;
  try 
  {
    const result = await model.generateContent(prompt);
    console.log('Raw result:', result); 
    if (result && result.response && result.response.candidates && result.response.candidates.length > 0) 
    {
      const parts = result.response.candidates[0].content.parts;
      const generatedText = parts.map(part => part.text).join('').trim(); // Join parts into a single string
      res.send(generatedText); 
    } 
    else 
    {
      res.json({ text: "No response from Google Generative AI." });
    }
  } 
  catch (error) 
  {
    console.error('Error generating content:', error);
    res.status(500).json({ error: 'Error generating content', details: error.message });
  }
});

app.listen(port, () => {
  console.log(`Service running at http://localhost:${port}`);
});
