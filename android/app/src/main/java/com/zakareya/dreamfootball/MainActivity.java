package com.zakareya.dreamfootball;
import android.app.Activity;
import android.os.Bundle;
import android.view.WindowManager;
import android.webkit.WebChromeClient;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.webkit.WebSettings;
import android.graphics.Color;
public class MainActivity extends Activity {
 private WebView gameView;
 @Override protected void onCreate(Bundle savedInstanceState) {
  super.onCreate(savedInstanceState);
  getWindow().setStatusBarColor(Color.rgb(7,27,23));
  getWindow().setNavigationBarColor(Color.rgb(7,27,23));
  getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
  gameView=new WebView(this); gameView.setBackgroundColor(Color.rgb(7,27,23));
  WebSettings s=gameView.getSettings(); s.setJavaScriptEnabled(true); s.setDomStorageEnabled(true);
  s.setDatabaseEnabled(true); s.setAllowFileAccess(true); s.setAllowContentAccess(true);
  s.setMediaPlaybackRequiresUserGesture(false); s.setLoadsImagesAutomatically(true);
  gameView.setWebChromeClient(new WebChromeClient()); gameView.setWebViewClient(new WebViewClient());
  setContentView(gameView); gameView.loadUrl("file:///android_asset/index.html");
 }
 @Override public void onBackPressed(){if(gameView!=null&&gameView.canGoBack())gameView.goBack();else super.onBackPressed();}
 @Override protected void onPause(){if(gameView!=null)gameView.onPause();super.onPause();}
 @Override protected void onResume(){super.onResume();if(gameView!=null)gameView.onResume();}
 @Override protected void onDestroy(){if(gameView!=null){gameView.destroy();gameView=null;}super.onDestroy();}
}