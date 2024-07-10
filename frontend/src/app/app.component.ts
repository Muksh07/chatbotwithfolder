import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterOutlet } from '@angular/router';

interface Message {
  text: string;
  isUser: boolean;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet,FormsModule,CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent 
{
  messages: Message[] = [];
  userMessage: string = '';
  folderStructure: any = null;
  selectedFileContent: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {}

  sendMessage() {
    if (this.userMessage.trim()) {
      this.messages.push({ text: this.userMessage, isUser: true });

      this.http.post<any>('http://localhost:5095/api/Chatbotbackend', { message: this.userMessage })
        .subscribe(response => {
          this.messages.push({ text: response.response, isUser: false });
          if (response.folderStructure) {
            this.folderStructure = response.folderStructure;
          }
        });

      this.userMessage = '';
    }
  }

  displayFileContent(content: string) {
    this.selectedFileContent = content;
  }

  toggleFolder(folder: any) {
    folder.expanded = !folder.expanded;
  }
}
