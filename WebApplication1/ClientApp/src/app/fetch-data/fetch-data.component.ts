import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource, MatTable } from '@angular/material/table';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html',
  styleUrls: ['./fetch-data.component.css']
})
export class FetchDataComponent implements OnInit {
  public articles: ArticleInfo[] = [];
  articlesRetrieved = 0;
  displayedColumns: string[] = ['title', 'url'];
  dataSource: MatTableDataSource<ArticleInfo>;

  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild(MatSort, { static: true }) sort: MatSort;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  ngOnInit() {
    // Get the list of new article IDs from the server
    this.http.get<ArticleList[]>(this.baseUrl + 'api/ArticleInfoes').subscribe((list) => {
      // For each ID get the title and URL (if it has one)
      list.forEach((id) => {
        this.http.get<ArticleInfo>('https://hacker-news.firebaseio.com/v0/item/' + id.id + '.json?print=pretty').subscribe(article => {
          this.articles.push({ id: article.id, title: article.title, url: !article.url ? 'No link provided' : article.url });
          this.articlesRetrieved++;
          if (this.articlesRetrieved === list.length) {
            this.updateDataSource();
          }
        });
      });
    },
      error => console.error(error));
  }

  updateDataSource() {
    // Assign the data to the data source for the table to render
    this.dataSource = new MatTableDataSource<ArticleInfo>(this.articles);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }
}

interface ArticleList {
  id: string;
}

interface ArticleInfo {
  id: string;
  title: string;
  url: string;
}
